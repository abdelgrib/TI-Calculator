using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RPNController : ControllerBase
    {
        /// <summary>
        /// The key here is the 'stack_id'.
        /// 'static' to keep state for different requests.
        /// </summary>
        private static IDictionary<string, Stack<int>> _stacks = new Dictionary<string, Stack<int>>();
        private static readonly IList<string> _operations = new List<string>
        {
            "+",
            "-",
            "*",
            "/",
        };
        private static readonly Random _random = new Random();

        public RPNController()
        {
        }

        [HttpGet("op")]
        public IActionResult GetOperations()
        {
            return Ok(_operations);
        }

        [HttpGet("stack")]
        public IActionResult GetStacks()
        {
            return Ok(_stacks.Keys);
        }

        [HttpGet("stack/{stack_id}", Name = "GetStackRoute")]
        public IActionResult GetStack(string stack_id)
        {
            if (!_stacks.ContainsKey(stack_id))
                return NotFound();

            var stack = _stacks[stack_id];
            return Ok(stack);
        }

        [HttpPost("stack")]
        public IActionResult CreateStack()
        {
            var stackId = Guid.NewGuid().ToString();
            var stack = new Stack<int>();
            _stacks.Add(stackId, stack);

            return CreatedAtRoute("GetStackRoute", new { stack_id = stackId }, stack.ToArray());
        }

        [HttpDelete("stack/{stack_id}")]
        public IActionResult DeleteStack(string stack_id)
        {
            if(!_stacks.ContainsKey(stack_id))
                return NotFound();

            _stacks.Remove(stack_id);

            return NoContent();
        }

        [HttpPost("stack/{stack_id}")]
        public IActionResult PushToStack(string stack_id)
        {
            if (!_stacks.ContainsKey(stack_id))
                return NotFound();

            var value = _random.Next(1, 10);
            var stack = _stacks[stack_id];
            stack.Push(value);

            return Ok(stack.ToArray());
        }

        [HttpPost("stack/{stack_id}/clear")]
        public IActionResult ClearStack(string stack_id)
        {
            if (!_stacks.ContainsKey(stack_id))
                return NotFound();

            var stack = _stacks[stack_id];
            stack.Clear();

            return Ok(stack.ToArray());
        }

        [HttpPost("op/{op}/stack/{stack_id}")]
        public IActionResult OperationOnStack(string op, string stack_id)
        {
            if (!_stacks.ContainsKey(stack_id))
                return NotFound();

            if(!_operations.Contains(op))
                return BadRequest();

            var stack = _stacks[stack_id];
            if(stack.Count < 2)
                return BadRequest();

            var x = stack.Pop();
            var y = stack.Pop();
            var (success, result) = ApplyOperation(x, y, op);
            if(!success)
                return BadRequest();

            stack.Push(result);

            return Ok(stack.ToArray());
        }

        private (bool Success, int Result) ApplyOperation(int x, int y, string operation)
        {
            switch (operation)
            {
                case "+":
                    return (true, x + y);
                case "-":
                    return (true, x - y);
                case "*":
                    return (true, x * y);
                case "/":
                    if (y == 0)
                        return (false, 0);
                    return (true, x / y);
                default:
                    return (false, 0);
            }
        }
    }
}
