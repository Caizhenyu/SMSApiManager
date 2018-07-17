using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMSApiManager.Authorization;
using SMSApiManager.Data;
using SMSApiManager.Models;
using SMSApiManager.Models.UserApiViewModels;

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

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserApiViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(model);
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
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }

            return Created("Detail", model);
        }

        [HttpGet("{query}")]
        public async Task<IActionResult> Detail(string query)
        {
            var userApi = _applicationDbContext.UserApi.Where(a => a.ApiNo == query || a.ApiName == query && a.Status != ApiStatus.NoUse).FirstOrDefault();

            if (userApi == null)
            {
                return NotFound("Check your query");
            }

            var isAuthorizedRead = await _authorizationService.AuthorizeAsync(User, userApi, ContactOperations.Read);

            if (!isAuthorizedRead.Succeeded)
            {
                return new ChallengeResult();
            }

            var result =  _mapper.Map<UserApiDetailViewModel>(userApi);

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
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }

            return NoContent();
        }

        //public async Task<IActionResult> 
    }
}