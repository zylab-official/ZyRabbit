﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZyRabbit.Messages.Sample;
using ZyRabbit.Operations.MessageSequence;

namespace ZyRabbit.AspNet.Sample.Controllers
{
	public class ValuesController : Controller
	{
		private readonly IBusClient _busClient;
		private readonly Random _random;
		private readonly ILogger<ValuesController> _logger;

		public ValuesController(IBusClient legacyBusClient, ILogger<ValuesController> logger)
		{
			_busClient = legacyBusClient;
			_logger = logger;
			_random = new Random();
		}

		[HttpGet]
		[Route("api/values")]
		public async Task<IActionResult> GetAsync()
		{
			_logger.LogDebug("Received Value Request.");
			var valueSequence = _busClient.ExecuteSequence(s => s
				.PublishAsync(new ValuesRequested
					{
						NumberOfValues = _random.Next(1,10)
					})
				.When<ValueCreationFailed, MessageContext>(
					(failed, context) =>
					{
						_logger.LogWarning("Unable to create Values. Exception: {0}", failed.Exception);
						return Task.FromResult(true);
					}, it => it.AbortsExecution())
				.Complete<ValuesCalculated>()
			);

			try
			{
				await valueSequence.Task;
			}
			catch (Exception e)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, $"No response received. Is the Console App started? \n\nException: {e}");
			}

			_logger.LogInformation("Successfully created {valueCount} values", valueSequence.Task.Result.Values.Count);

			return Ok(valueSequence.Task.Result.Values);
		}

		[HttpGet("api/values/{id}")]
		public async Task<string> GetAsync(int id)
		{
			_logger.LogInformation("Requesting Value with id {valueId}", id);
			var response = await _busClient.RequestAsync<ValueRequest, ValueResponse>(new ValueRequest {Value = id});
			return response.Value;
		}
	}
}
