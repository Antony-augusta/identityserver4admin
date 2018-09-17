using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMCApp.Constants;
using FMCApp.Core.Dtos.Grant;
using FMCApp.Core.Services;
using FMCApp.ExceptionHandling;
using FMCApp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FMCApp.Controllers
{
    [Authorize(Policy = AuthorizationConsts.AdministrationPolicy)]
    [TypeFilter(typeof(ControllerExceptionFilterAttribute))]
    public class GrantController : BaseController
    {
        private readonly IPersistedGrantService _persistedGrantService;
        private readonly IStringLocalizer<GrantController> _localizer;

        public GrantController(IPersistedGrantService persistedGrantService,
            ILogger<ConfigurationController> logger,
            IStringLocalizer<GrantController> localizer) : base(logger)
        {
            _persistedGrantService = persistedGrantService;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> PersistedGrants(int? page, string search)
        {
            ViewBag.Search = search;
            var persistedGrants = await _persistedGrantService.GetPersitedGrantsByUsers(search, page ?? 1);

            return View(persistedGrants);
        }

        [HttpGet]
        public async Task<IActionResult> PersistedGrantDelete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var grant = await _persistedGrantService.GetPersitedGrantAsync(UrlHelpers.QueryStringUnSafeHash(id));
            if (grant == null) return NotFound();

            return View(grant);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PersistedGrantDelete(PersistedGrantDto grant)
        {
            await _persistedGrantService.DeletePersistedGrantAsync(grant.Key);

            SuccessNotification(_localizer["SuccessPersistedGrantDelete"], _localizer["SuccessTitle"]);

            return RedirectToAction(nameof(PersistedGrants));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PersistedGrantsDelete(PersistedGrantsDto grants)
        {
            await _persistedGrantService.DeletePersistedGrantsAsync(grants.SubjectId);

            SuccessNotification(_localizer["SuccessPersistedGrantsDelete"], _localizer["SuccessTitle"]);

            return RedirectToAction(nameof(PersistedGrants));
        }


        [HttpGet]
        public async Task<IActionResult> PersistedGrant(string id, int? page)
        {
            var persistedGrants = await _persistedGrantService.GetPersitedGrantsByUser(id, page ?? 1);
            persistedGrants.SubjectId = Convert.ToInt16(id);

            return View(persistedGrants);
        }
    }
}
