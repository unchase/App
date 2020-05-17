using System;
using System.Threading.Tasks;
using DotNetRu.AzureService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotNetRu.Azure
{
    [Route("")]
    public class VkontakteController : ControllerBase
    {
        private readonly ILogger logger;

        private readonly VkontakteSettings vkontakteSettings;

        public VkontakteController(
            ILogger<DiagnosticsController> logger,
            VkontakteSettings vkontakteSettings)
        {
            this.logger = logger;
            this.vkontakteSettings = vkontakteSettings;
        }

        [HttpGet]
        [Route("VkontaktePosts")]
        public async Task<IActionResult> GetOriginalPosts()
        {
            try
            {
                //ToDo: добавить кэширование результатов, чтобы не исчерпать лимит VK API
                var posts = await VkontakteService.GetAsync(this.vkontakteSettings);
                var json = JsonConvert.SerializeObject(posts);

                return new OkObjectResult(json);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Unhandled error while getting original vkontakte posts");
                return new ObjectResult(e)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
