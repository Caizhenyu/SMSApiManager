using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SMSApiManager.Data;
using SMSApiManager.Models;

namespace SMSApiManager.Controllers.Memberc
{
    [AllowAnonymous]
    [Produces("application/json")]
    [Route("api/Member")]
    public class MemberController : Controller
    {
        private ApplicationDbContext db;

        public MemberController(ApplicationDbContext _db)
        {
            db = _db;
        }

        /// <summary>
        /// 新增一个党员
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        [HttpPost("addMember")]
        public ActionResult AddMenber(Member member)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Member _member = new Member()
            {
                MemberNo = member.MemberNo,
                Name = member.Name,
                Birthday = member.Birthday,
                PhoneNumber = member.PhoneNumber,
                Status = MemberStatus.Normal,
                Email = member.Email,
                Address = member.Address,
                OwnerID = member.OwnerID,
            };
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
        public ActionResult UpdateMember(string memberNo, string memberName, DateTime birthday, string phone)
        {
            if (string.IsNullOrEmpty(memberNo))
            {
                return BadRequest("党员编号不能为空!");
            }
            var mem = db.Member.SingleOrDefault(x => x.MemberNo == memberNo);
            if (mem == null)
            {
                return BadRequest("查无此人");
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
        /// <summary>
        /// 上传EXCel
        /// </summary>
        /// <returns></returns>
        [HttpPost("Import")]
        public IActionResult Import(IFormFile excelfile)
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

                return BadRequest("对不起，xls|xlsx类型的文件");
            }
            string newname = DateTime.Now.ToString("yyyyMMddmmss") + new Random().Next(9999) + "." + ExtenName;
            var path = Path.Combine(@"upload\xls", newname);
            var p = System.IO.Path.GetFullPath(path);
            //string sFileName = $"{Guid.NewGuid()}.xlsx";
            FileInfo file = new FileInfo(Path.Combine(@"upload\xls", newname));
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
        [HttpPost("delectMemberList")]
        public IActionResult DelectMemberList(List<string> memberNos)
        {
            var list = memberNos;
            foreach (var memNo in memberNos)
            {
                string test =memNo;
            }
            return Ok("删除成功");
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