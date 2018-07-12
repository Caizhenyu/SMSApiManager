using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SMSApiManager.Data;
using SMSApiManager.Models;
using SMSApiManager.Models.MemberViewModles;

namespace SMSApiManager.Controllers.Memberc
{
    [AllowAnonymous]
    [Produces("application/json")]
    [Route("api/Member")]
    public class MemberController : Controller
    {
        private ApplicationDbContext db;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(ApplicationDbContext _db, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            db = _db;
            _mapper = mapper;
            _userManager = userManager;
        }

        /// <summary>
        /// 新增一个党员
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        [HttpPost("addMember")]
        public async Task<IActionResult> AddMenber(MemberView member)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var _member = _mapper.Map<Member>(member);
            //Member _member = new Member()
            //{
            //    MemberNo = member.MemberNo,
            //    Name = member.Name,
            //    Birthday = member.Birthday,
            //    PhoneNumber = member.PhoneNumber,
            //    Status = MemberStatus.Normal,
            //    Email = member.Email,
            //    Address = member.Address,
            //    OwnerID = member.OwnerID,
            //};
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            _member.OwnerID = user.Id;
            db.Member.Add(_member);
            db.SaveChanges();
            return Ok("保存成功!");
        }
        /// <summary>
        /// 查看党员信息
        /// </summary>
        /// <param name="memberNo"></param>
        /// <returns></returns>
        [HttpGet("getMember")]
        public ActionResult GetMember(string memberNo)
        {
            if (!string.IsNullOrEmpty(memberNo))
            {

                var mem = db.Member.SingleOrDefault(x => x.MemberNo == memberNo);
                if (mem != null)
                {
                    M m = new M()
                    {
                        Name =mem.Name,
                        phoneNumber =mem.PhoneNumber,
                        Birthday =mem.Birthday.ToString("yyyy-MM-dd"),
                        Age =GetAge(mem.Birthday),
                        memberNO= mem.MemberNo
                    };
                    return Ok(m);
                }
                else
                {
                    return BadRequest("查无党员");
                }
            }
            else
            {
                return BadRequest("编号为空！");
            }
        }

        /// <summary>
        /// 更新党员信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("updateMember")]
        public ActionResult UpdateMember(int memberID, string memberName, DateTime birthday, string phone)
        {
            try
            {
                var mem = db.Member.SingleOrDefault(x =>x.MemberId ==memberID);
                if (mem == null)
                {
                    return BadRequest("查无党员信息");
                }
                if (!string.IsNullOrEmpty(memberName))
                {
                    mem.Name = memberName;
                }
                if (!string.IsNullOrEmpty(phone))
                {
                    mem.PhoneNumber = phone;
                }
                if (birthday != null)
                {
                    mem.Birthday = birthday;
                }
                db.SaveChanges();
                return Ok("修改成功");
            }
            catch (Exception)
            {

                throw new Exception("党员ID出错");
            }
            
        }
        /// <summary>
        /// 上传EXCel
        /// </summary>
        /// <returns></returns>
        [HttpPost("Import")]
        public async Task<IActionResult>  Import(IFormFile excelfile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("文件为空");
            }
            var Files = Request.Form.Files;
            var fil = Files.FirstOrDefault();
            string fileName = fil.FileName;
            //string sWebRootFolder = _hostingEnvironment.WebRootPath;
            string ExtenName = fileName.Substring(fileName.LastIndexOf(".") + 1);
            if (!Regex.IsMatch(ExtenName, "(?s)|xls|xlsx(?-s)", RegexOptions.IgnoreCase))
            {

                return BadRequest("对不起，只允许上传xls或xlsx类型的文件");
            }
            string newname = DateTime.Now.ToString("yyyyMMddmmss") + new Random().Next(9999) + "." + ExtenName;
            var path = Path.Combine(@"wwwroot\upload\xls", newname);
            var p = System.IO.Path.GetFullPath(path);
            //string sFileName = $"{Guid.NewGuid()}.xlsx";
            FileInfo file = new FileInfo(Path.Combine(@"wwwroot\upload\xls", newname));
            try
            {
                using (FileStream fs = new FileStream(file.ToString(), FileMode.Create))
                {
                    fil.CopyTo(fs);
                    fs.Flush();
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    StringBuilder sb = new StringBuilder();
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int rowCount = worksheet.Dimension.Rows;
                    int ColCount = worksheet.Dimension.Columns;
                    bool bHeaderRow = true;
                    string s = string.Empty;
                    int a = 0;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        Member d = new Member();
                        for (int col = 2; col <= ColCount; col++)
                        {
                            if (bHeaderRow)
                            {
                                sb.Append(worksheet.Cells[row, col].Value.ToString() + "\t");
                                s = worksheet.Cells[row, col].Value.ToString();
                                var user = await _userManager.FindByEmailAsync(User.Identity.Name);
                                d.OwnerID = user.Id;
                                switch (col)
                                {
                                    case 2:
                                        d.MemberNo = s;
                                        break;
                                    case 3:
                                        d.Name = s;
                                        break;
                                    case 4:
                                        d.Birthday = Convert.ToDateTime(s);
                                        break;
                                    case 5:
                                        d.PhoneNumber = s;
                                        break;
                                    default:
                                        break;
                                }
                                d.Status = MemberStatus.Normal;
                            }
                            else
                            {
                                sb.Append(worksheet.Cells[row, col].Value.ToString() + "\t");
                            }
                        }
                        a += 1;
                        db.Member.Add(d);
                        db.SaveChanges();
                        sb.Append(Environment.NewLine);
                    }
                    return Content("共导入了" + a + "条数据");
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        /// <summary>
        /// 删除党员
        /// </summary>
        /// <param name="memberNo"></param>
        /// <returns></returns>
        [HttpGet("delectMember")]
        public IActionResult DelectMember(string memberNo)
        {
            if (string.IsNullOrEmpty(memberNo))
            {
                return BadRequest("党员编号不能为空");
            }
            var menbetr = db.Member.FirstOrDefault(x => x.MemberNo == memberNo);
            if (menbetr == null)
            {
                return BadRequest("找不到该党员");
            }
            menbetr.Status = MemberStatus.NoUse;
            db.SaveChanges();
            return Ok("删除成功");
        }
        /// <summary>
        /// 获取党员列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sort"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        [HttpGet("getMembetList")]
        public IActionResult getMemberList(int pageSize,int pageIndex, string sort, string searchValue)
        {
            var memberList = (from a in db.Member.Where(x => x.Status == MemberStatus.Normal).Skip(pageSize * (pageIndex - 1))
                             .Take(pageSize)
                              select new
                              {
                                  a.MemberNo,
                                  a.Name,
                                  a.Birthday,
                                  a.PhoneNumber
                              }).ToList().Select(a => new
                              {
                                  a.MemberNo,
                                  a.Name,
                                  a.Birthday,
                                  age = GetAge(a.Birthday),
                                  a.PhoneNumber
                              });

            if (memberList != null)
            {

                //int age = Convert.ToInt32(searchValue);
                //memberList = memberList.Where(x =>x.age ==age);

                if (!string.IsNullOrEmpty(searchValue))
                {
                    memberList = memberList.Where(x => x.Name.Contains(searchValue) || x.PhoneNumber.Contains(searchValue));
                }
                switch (sort)
                {
                    case "1":
                        memberList = memberList.OrderByDescending(x => x.age);
                        break;
                    case "2":
                        memberList = memberList.OrderBy(x => x.age);
                        break;
                };
                return Ok(memberList.ToList());
            }
            else
            {
                return BadRequest("暂无数据！");
            }
        }
        /// <summary>
        /// 批量删除党员
        /// </summary>
        /// <param name="memberNos"></param>
        /// <returns></returns>
        [HttpPost("delectMemberList")]
        public IActionResult DelectMemberList(List<int> memberIDs)
        {

            Member member = null;
            if (memberIDs.Count != 0)
            {
                foreach (int id in memberIDs)
                {
                    try
                    {
                        member = db.Member.SingleOrDefault(x => x.MemberId == id);
                        if (member != null)
                        {
                            member.Status = MemberStatus.Normal;
                        }

                    }
                    catch (Exception)
                    {

                        throw new Exception("党员ID出错,删除失败");
                    }

                    db.SaveChanges();

                }
                return Ok("删除成功");
            }
            else
            {
                return BadRequest("参数错误!删除失败！");
            }
           
        }

        /// <summary>
        /// 计算年龄
        /// </summary>
        /// <param name="birthday"></param>
        /// <returns></returns>
        private int GetAge(DateTime birthday)
        {
            DateTime time = DateTime.Now;
            int age = time.Year - birthday.Year;
            // 修正:当前日期的月/日没有到出生年份的月/日
            if (birthday > time.AddYears(-age))
            {
                age--;
            }
            return age;
        }
    }
    class M
    {
        public string memberNO { set; get; }
        public string phoneNumber { set; get; }

        public int Age { set; get; }

        public string Birthday { set; get; }

        public string Name { set; get; }
    }
}