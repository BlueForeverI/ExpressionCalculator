using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ExpressionCalculator.Models;

namespace ExpressionCalculator.Controllers
{
    public class ExpressionsController : ApiController
    {
        [ActionName("calculate")]
        [HttpPost]
        public HttpResponseMessage CalculateExpressions([FromBody]ExpressionModel expressionModel)
        {
            var response = new HttpResponseMessage();
            try
            {
                ReversePolishNotationEvaluator evaluator = new ReversePolishNotationEvaluator();
                evaluator.Parse(expressionModel.Expression);
                expressionModel.Result = evaluator.Evaluate();
                response = this.Request.CreateResponse(HttpStatusCode.OK, expressionModel);
            }
            catch (Exception)
            {
                response = this.Request.CreateResponse(
                    HttpStatusCode.BadRequest, "Error calculating the expression");
            }

            return response;
        }
    }
}
