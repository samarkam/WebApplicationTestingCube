using Microsoft.AspNetCore.Mvc;
using Microsoft.AnalysisServices.AdomdClient;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace WebApplicationTestingCube.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSASController : ControllerBase
    {


        private static readonly string CNX_STRING = "Provider=MSOLAP; Data Source=localhost;Provider=MSOLAP;Initial Catalog=MultidimensionalProject projet DS BI;";

        /// <summary>
        /// Executes an MDX query and returns the result as JSON.
        /// </summary>
        /// <param name="mdxQuery">The MDX query to execute.</param>
        /// <returns>Structured JSON result of the query.</returns>
        [HttpPost("executeQuery")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public IActionResult ExecuteQuery([FromBody] QueryRequest request)
        {
            var mdxQuery = request.MdxQuery;
            if (string.IsNullOrWhiteSpace(mdxQuery))
            {
                return BadRequest(new { message = "MDX query cannot be null or empty." });
            }

            AdomdConnection conn = null;
            try
            {
                conn = new AdomdConnection(CNX_STRING);
                conn.Open();

                using (AdomdCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = mdxQuery;
                    var cellSet = cmd.ExecuteCellSet();

                    var result = ParseCellSet(cellSet);
                    return Ok(result);
                }
            }
            catch (AdomdConnectionException ex)
            {
                return StatusCode(500, new { message = "Failed to connect to SSAS.", error = ex.Message });
            }
            catch (AdomdErrorResponseException ex)
            {
                return StatusCode(500, new { message = "Error executing MDX query.", error = ex.Message });
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        [HttpPost("ExecuteQueryForMap")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public IActionResult ExecuteQueryForMap([FromBody] QueryRequest request)
        {
            var mdxQuery = request.MdxQuery;
            if (string.IsNullOrWhiteSpace(mdxQuery))
            {
                return BadRequest(new { message = "MDX query cannot be null or empty." });
            }

            AdomdConnection conn = null;
            try
            {
                conn = new AdomdConnection(CNX_STRING);
                conn.Open();

                using (AdomdCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = mdxQuery;
                    var cellSet = cmd.ExecuteCellSet();

                    var result = ParseCellSetForMap(cellSet);
                    return Ok(result);
                }
            }
            catch (AdomdConnectionException ex)
            {
                return StatusCode(500, new { message = "Failed to connect to SSAS.", error = ex.Message });
            }
            catch (AdomdErrorResponseException ex)
            {
                return StatusCode(500, new { message = "Error executing MDX query.", error = ex.Message });
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
    
        [HttpGet("execute/query/1")]
        public IActionResult ExecuteQueryOne()
        {
            var mdxQuery = "SELECT ([Calender].[Year].&[2011-01-01T00:00:00] : [Calender].[Year].&[2014-01-01T00:00:00]) ON COLUMNS, [Measures].[Line Total] ON ROWS FROM [BI DB DS]";
            return ExecuteQuery(new QueryRequest { MdxQuery = mdxQuery });
        }

   
        [HttpGet("execute/query/2")]
        public IActionResult ExecuteQueryTwo()
        {
            var mdxQuery = "SELECT NON EMPTY  { [Calender].[Month].[Month] }  ON COLUMNS, NON EMPTY {[Measures].[AverageLineTotal] } ON ROWS FROM [BI DB DS] \r\n";
            return ExecuteQuery(new QueryRequest { MdxQuery = mdxQuery });
        }


        [HttpGet("execute/query/3")]
        public IActionResult ExecuteQueryThree()
        {
            var mdxQuery = "SELECT     [Sales Territory].[Territory].[Territory].MEMBERS ON COLUMNS,   ([Calender].[Year].&[2013-01-01T00:00:00], [Measures].[Line Total]) ON ROWS FROM [BI DB DS]\r\n";
            return ExecuteQueryForMap(new QueryRequest { MdxQuery = mdxQuery });
        }


        [HttpGet("execute/query/4")]
        public IActionResult ExecuteQueryFour()
        {
            var mdxQuery = "SELECT   NON EMPTY  TopCount([Product].[Product].[Product].MEMBERS, 10, [Measures].[AverageLineTotal]) ON COLUMNS, NON EMPTY    [Measures].[AverageLineTotal] ON ROWS FROM [BI DB DS]";
            return ExecuteQuery(new QueryRequest { MdxQuery = mdxQuery });
        }


        public class QueryRequest
        {
            public string MdxQuery { get; set; }
        }
        private object ParseCellSet(CellSet cellSet)
        {
            var data = new List<Dictionary<string, object>>();
            var columns = new List<string>();

            foreach (var tuple in cellSet.Axes[0].Set.Tuples)
            {
                columns.Add(tuple.Members[0].Caption);
            }

            var rows = cellSet.Axes[1].Set.Tuples;
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = new Dictionary<string, object>();

                row["RowHeader"] = rows[rowIndex].Members[0].Caption;

                for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    row[columns[colIndex]] = cellSet.Cells[colIndex, rowIndex].FormattedValue;
                }

                data.Add(row);
            }

            return new { Columns = columns, Data = data };
        }

        private object ParseCellSetForMap(CellSet cellSet)
        {
            var data = new List<Dictionary<string, object>>();
            var columns = new List<string>();

            // Get columns (Territory names)
            foreach (var tuple in cellSet.Axes[0].Set.Tuples)
            {
                columns.Add(tuple.Members[0].Caption); // Territory names (e.g., Northwest, Canada)
            }

            // Get rows (Line Total values for the territories)
            var rows = cellSet.Axes[1].Set.Tuples;
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = new Dictionary<string, object>();
                row["RowHeader"] = rows[rowIndex].Members[0].Caption; // Calendar year (e.g., 2013)

                // Get data for each territory in the columns
                for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    // Use the formatted value for the corresponding cell
                    row[columns[colIndex]] = string.IsNullOrEmpty(cellSet.Cells[colIndex, rowIndex].FormattedValue)
                        ? 0
                        : cellSet.Cells[colIndex, rowIndex].FormattedValue;
                }

                data.Add(row);
            }

            return new { Columns = columns, Data = data };
        }

       


    }
    }
