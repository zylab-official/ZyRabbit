using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZyRabbit.Messages.Sample;
using ZyRabbit.Operations.MessageSequence;

namespace ZyRabbit.AspNet.Sample.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ValuesController : ControllerBase
	{
		private readonly IBusClient _busClient;
		private readonly Random _random = new Random();
		private readonly ILogger<ValuesController> _logger;

		public ValuesController(IBusClient busClient, ILogger<ValuesController> logger)
		{
			_busClient = busClient;
			_logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> GetAsync()
		{
			_logger.LogDebug("Received Value Request.");

			try
			{
				var valueSequenceTask = _busClient.ExecuteSequence(s => s
					.PublishAsync(new ValuesRequested
					{
						NumberOfValues = _random.Next(1, 10)
					})
					.When<ValueCreationFailed, MessageContext>(
						(failed, context) =>
						{
							_logger.LogWarning("Unable to create Values. Exception: {0}", failed.Exception);
							return Task.FromResult(true);
						}, it => it.AbortsExecution())
					.Complete<ValuesCalculated>());

				var valueSequence = await valueSequenceTask.Task;

				_logger.LogInformation("Successfully created {valueCount} values", valueSequence.Values.Count);

				return Ok(valueSequence.Values);
			}
			catch (Exception e)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, $"No response received. Is the Console App started? \n\nException: {e}");
			}
		}

		[HttpGet("{id}")]
		public async Task<string> GetAsync(int id)
		{
			_logger.LogInformation("Requesting Value with id {valueId}", id);
			var response = await _busClient.RequestAsync<ValueRequest, ValueResponse>(new ValueRequest {Value = id});
			return response.Value;
		}
	}
}
