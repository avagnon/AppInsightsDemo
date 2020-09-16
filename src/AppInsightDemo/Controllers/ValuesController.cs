﻿using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppInsightDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;
        private readonly TelemetryClient _telemetryClient;

        public ValuesController(ILogger<ValuesController> logger, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Adds additional properties to a Request Telemetry using HttpContext.Features 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/demo1")]
        public ActionResult<IEnumerable<string>> HttpContextFeaturesDemo()
        {
            var requestTelemetry = HttpContext.Features.Get<RequestTelemetry>();
            requestTelemetry.Properties.Add("aProperty1", "setUsingFeature");

            _telemetryClient.TrackEvent("NoProperty1");

            using (_telemetryClient.StartOperation<DependencyTelemetry>("aSubOperationOfHttpContextFeaturesDemo"))
            {
                // This dependency telemetry item won't have the custom property
            }

            return new[] { "value1", "value2" };
        }

        /// <summary>
        /// Adds additional properties to a telemetry of same operation using Activity
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/demo2")]
        public ActionResult<IEnumerable<string>> ActivityDemo()
        {
            Activity.Current.AddBaggage("aProperty2", "setUsingActivityBaggage");   // Add aProperty3 to sub operations of the request telemetry item only

            _telemetryClient.TrackEvent("WithProperty2");

            using (_telemetryClient.StartOperation<DependencyTelemetry>("aSubOperationOfActivityDemo"))
            {
                // This dependency telemetry will have only the properties set using Activity.Current.AddBaggage(..)

                // This event telemetry will have only the properties set using Activity.Current.AddBaggage(..)
                _telemetryClient.TrackEvent("WithProperty2InsideSubOp");
            }

            return new[] { "value1", "value2" };
        }

        /// <summary>
        /// Adds additional properties to a trace telemetry using log scopes
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/demo3")]
        public ActionResult<IEnumerable<string>> BeginScopeDemo()
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                {"aProperty3", "setUsingScope"}
            }))
            {
                _logger.LogWarning("Some Warning");
            }

            return new[] { "value1", "value2" };
        }
    }
}
