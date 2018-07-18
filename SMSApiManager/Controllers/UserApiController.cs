using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SMSApiManager.Authorization;
using SMSApiManager.Data;
using SMSApiManager.Extensions;
using SMSApiManager.Models;
using SMSApiManager.Models.Common;
using SMSApiManager.Models.UserApiViewModels;
using SMSApiManager.Models.ViewModels;

namespace SMSApiManager.Controllers
{
    //[AllowAnonymous]
    //[Authorize]
    [Authorize("Permission")]
    [Route("api/UserApi")]
    public class UserApiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public UserApiController(ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IAuthorizationService authorizationService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _mapper = mapper;
            _authorizationService = authorizationService;
        }

        #region Create

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserApiViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ModelState);
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var result = _applicationDbContext.UserApi.FirstOrDefault(a => a.ApiNo == model.ApiNo && a.OwnerId == user.Id);

            if (result != null)
            {
                return StatusCode(StatusCodes.Status409Conflict, $"Data already exists, status is {result.Status.ToString()}");
            }

            var userApi = _mapper.Map<UserApi>(model);
            userApi.OwnerId = user.Id;

            _applicationDbContext.UserApi.Add(userApi);

            try
            {
                await _applicationDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.Message);
            }

            return Created("Detail", model);
        }

        /// <summary>
        /// Create a Api Collection
        /// </summary>
        /// <param name="userApiModels">A CreateUserApiViewModel Collection</param>
        /// <returns></returns>
        [HttpPost("ApiCollection")]
        public async Task<IActionResult> CreateCollection([FromBody]IEnumerable<CreateUserApiViewModel> userApiModels)
        {
            if (userApiModels == null)
            {
                return BadRequest();
            }

            var userApis = _mapper.Map<IEnumerable<UserApi>>(userApiModels).ToList();
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var existsUserApi = _applicationDbContext.UserApi.Where(a => a.OwnerId == user.Id);

            //记录添加结果
            IList<ResultViewModel> results = new List<ResultViewModel>(userApis.Count());

            //记录本次需要添加到数据库的 ApiNo 列表
            IList<string> apiNos = new List<string>(userApis.Count());

            for (int i = 0; i < userApis.Count(); i++)
            {
                var userApi = userApis[i];

                //验证 Model
                //针对情形：传入的集合中有 Invalid Model
                if (!ModelState.IsValid)
                {
                    //若是 Invalid Model ，跳出本次循环
                    if (ModelState.GetErrors().Where(e => e.Key.Contains($"[{i}")).Count() > 0)
                    {
                        var errors = ModelState.GetErrors().Where(e => e.Key.Contains($"[{i}")).ToList();
                        results.Add(new ResultViewModel(userApi.ApiNo, false, errors));
                        continue;
                    }
                }

                //避免重复添加
                //若已存在该 Model，跳出本次循环
                if (existsUserApi.Where(a => a.ApiNo == userApi.ApiNo).Count() > 0)
                {
                    results.Add(new ResultViewModel(userApi.ApiNo, false, $"{userApi.ApiNo} already exists, status is {existsUserApi.Single(a => a.ApiNo == userApi.ApiNo).Status.ToString()}"));
                    continue;
                }

                userApi.OwnerId = user.Id;
                _applicationDbContext.UserApi.Add(userApi);
                apiNos.Add(userApi.ApiNo);
            }

            if (apiNos.Count() > 0)
            {
                try
                {
                    _applicationDbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    results.Add(new ResultViewModel(string.Join(",", apiNos), false, "No Api Created", ex.InnerException.Message));
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        results);
                }

                foreach (var apiNo in apiNos)
                {
                    results.Add(new ResultViewModel(apiNo, true));
                }

                return Created("Index", results);
            }
            else
            {
                return Ok(results);
            }

        }

        #endregion

        #region Read

        [HttpGet]
        public async Task<IActionResult> Get(Pagination page)
        {
            var propertiesMap = new Dictionary<string, Expression<Func<UserApi, object>>>
            {
                {"ApiNo", a => a.ApiNo },
                {"ApiName", a => a.ApiName },
                {"Address", a => a.Address },
                {"Status", a => a.Status }
            };

            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            var query = _applicationDbContext.UserApi.Where(a => a.Status != ApiStatus.NoUse && a.OwnerId.Contains(user.Id)).AsQueryable();

            //简单搜索
            if (!string.IsNullOrEmpty(page.SearchValue))
            {
                query = query.Where(a => a.ApiNo.Contains(page.SearchValue) || a.ApiName.Contains(page.SearchValue) || a.Address.Contains(page.SearchValue) || a.Remark.Contains(page.SearchValue) || a.Status.ToString().Contains(page.SearchValue));
            }
            //排序
            if (!string.IsNullOrEmpty(page.OrderBy) && propertiesMap.Keys.Contains(page.OrderBy.TrimEnd(" Desc".ToArray())))
            {
                if (page.OrderBy.EndsWith(" Desc"))
                {
                    var property = page.OrderBy.Replace(" Desc", "");
                    query = query.OrderByDescending(propertiesMap[property]);
                }
                else
                {
                    query = query.OrderBy(propertiesMap[page.OrderBy]);
                }
            }

            var itemsCount = query.Count();
            var pageCount = itemsCount / page.PageSize + (itemsCount % page.PageSize > 0 ? 1 : 0);

            var userApis = query.Skip(page.PageIndex * page.PageSize).Take(page.PageSize).ToList();

            if (userApis == null || userApis.Count() == 0)
            {
                return NotFound();
            }            
            
            var meta = new
            {
                TotalItemsCount = itemsCount,
                PageCount = pageCount,
                HasPrevious = page.PageIndex > 0,
                HasNext = page.PageIndex < pageCount - 1
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta));

            var result = _mapper.Map<IEnumerable<UserApiDetailViewModel>>(userApis);
            return Ok(result);
        }

        [HttpGet("{query}")]
        public async Task<IActionResult> Detail(string query)
        {
            var userApi = _applicationDbContext.UserApi.Where(a => (a.ApiNo == query || a.ApiName == query) && a.Status != ApiStatus.NoUse).FirstOrDefault();

            if (userApi == null)
            {
                return NotFound("Check your query");
            }

            var isAuthorizedRead = await _authorizationService.AuthorizeAsync(User, userApi, ContactOperations.Read);

            if (!isAuthorizedRead.Succeeded)
            {
                return new ChallengeResult();
            }

            var result = _mapper.Map<UserApiDetailViewModel>(userApi);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var userApi = _applicationDbContext.UserApi.Where(a => a.Id == id && a.Status != ApiStatus.NoUse).FirstOrDefault();

            if (userApi == null)
            {
                return NotFound("Check your query");
            }

            var isAuthorizedRead = await _authorizationService.AuthorizeAsync(User, userApi, ContactOperations.Read);

            if (!isAuthorizedRead.Succeeded)
            {
                return new ChallengeResult();
            }

            var result = _mapper.Map<UserApiDetailViewModel>(userApi);

            return Ok(result);
        }

        #endregion

        #region Update

        [HttpPatch("{id:int}/Name")]
        public async Task<IActionResult> UpdateName(int id, string name)
        {
            var userApi = _applicationDbContext.UserApi.SingleOrDefault(a => a.Id == id);
            if(userApi == null)
            {
                return NotFound();
            }
            if (userApi.ApiName == name)
            {
                return Ok();
            }
            if (string.IsNullOrEmpty(name))
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, "Empty Name");
            }

            var isAuthorizedUpdate = await _authorizationService.AuthorizeAsync(User, userApi, ContactOperations.Update);

            if (!isAuthorizedUpdate.Succeeded)
            {
                return new ChallengeResult();
            }

            userApi.ApiName = name;

            try
            {
                _applicationDbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.Message);
            }

            var result = _mapper.Map<UserApiDetailViewModel>(userApi);
            return Ok(result);
        }

        [HttpPatch("{id:int}/Status")]
        public async Task<IActionResult> UpdateStatus(int id, int status)
        {
            var userApi = _applicationDbContext.UserApi.SingleOrDefault(a => a.Id == id);
            if (userApi == null)
            {
                return NotFound();
            }
            if ((int)userApi.Status == status)
            {
                return Ok();
            }
            if (!Enum.IsDefined(typeof(ApiStatus), status) || status == (int)ApiStatus.NoUse)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, "Invalid Status");
            }            

            var isAuthorizedUpdate = await _authorizationService.AuthorizeAsync(User, userApi, ContactOperations.Update);

            if (!isAuthorizedUpdate.Succeeded)
            {
                return new ChallengeResult();
            }

            userApi.Status = (ApiStatus)Enum.ToObject(typeof(ApiStatus), status);

            try
            {
                _applicationDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.Message);
            }

            var result = _mapper.Map<UserApiDetailViewModel>(userApi);
            return Ok(result);
        }

        [HttpPatch("{id:int}/Remark")]
        public async Task<IActionResult> UpdateRemark(int id, string remark)
        {
            var userApi = _applicationDbContext.UserApi.SingleOrDefault(a => a.Id == id);
            if (userApi == null)
            {
                return NotFound();
            }
            if (userApi.Remark == remark)
            {
                return Ok();
            }

            var isAuthorizedUpdate = await _authorizationService.AuthorizeAsync(User, userApi, ContactOperations.Update);

            if (!isAuthorizedUpdate.Succeeded)
            {
                return new ChallengeResult();
            }

            userApi.Remark = remark;

            try
            {
                _applicationDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.Message);
            }

            var result = _mapper.Map<UserApiDetailViewModel>(userApi);
            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateUserApiViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ModelState);
            }

            if(!Enum.IsDefined(typeof(ApiStatus), model.Status))
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, "Invalid Status");
            }

            var userApi = _applicationDbContext.UserApi.FirstOrDefault(a => a.Id == id);
            if(userApi == null)
            {
                return NotFound();
            }

            var isAuthorizedUpdate = await _authorizationService.AuthorizeAsync(User, userApi, ContactOperations.Update);

            if (!isAuthorizedUpdate.Succeeded)
            {
                return new ChallengeResult();
            }

            userApi.ApiName = string.IsNullOrEmpty(model.ApiName) ? userApi.ApiName : model.ApiName;
            userApi.Status = model.Status == ApiStatus.NoUse ? userApi.Status : model.Status;
            userApi.Remark = model.Remark;

            try
            {
                _applicationDbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.Message);
            }

            var result = _mapper.Map<UserApiDetailViewModel>(userApi);
            return Ok(result);
        }

        #endregion

        #region Delete

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userApi = _applicationDbContext.UserApi.Where(a => a.Id == id).FirstOrDefault();

            if (userApi == null)
            {
                return NotFound();
            }

            var isAuthorizedDelete = await _authorizationService.AuthorizeAsync(User, userApi, ContactOperations.Delete);

            if (!isAuthorizedDelete.Succeeded)
            {
                return new ChallengeResult();
            }

            userApi.Status = ApiStatus.NoUse;

            try
            {
               await  _applicationDbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.Message);
            }

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody]IEnumerable<DeleteUserApiViewModel> ids)
        {
            IList<ResultViewModel> results = new List<ResultViewModel>(ids.Count());
            IList<int> apiIds = new List<int>(ids.Count());

            foreach(var model in ids)
            {
                var userApi = _applicationDbContext.UserApi.Where(a => a.Id == model.Id).FirstOrDefault();

                if (userApi == null)
                {
                    results.Add(new ResultViewModel(model.Id, false, $"404 Not Found {model.Id}"));
                    continue;
                }

                var isAuthorizedDelete = await _authorizationService.AuthorizeAsync(User, userApi, ContactOperations.Delete);

                if (!isAuthorizedDelete.Succeeded)
                {
                    results.Add(new ResultViewModel(model.Id, false, $"401 Unauthorized {model.Id}"));
                    continue;
                }

                userApi.Status = ApiStatus.NoUse;
                apiIds.Add(model.Id);
            }

            if(apiIds.Count() > 0)
            {
                try
                {
                    _applicationDbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    results.Add(new ResultViewModel(string.Join(";", apiIds), false, "No Api Deleted", ex.InnerException.Message));
                    return StatusCode(StatusCodes.Status500InternalServerError,
                            results);
                }

                foreach (var id in apiIds)
                {
                    results.Add(new ResultViewModel(id, true));
                }                
            }

            return Ok(results);
        }

        #endregion

        //public async Task<IActionResult> 
    }
}