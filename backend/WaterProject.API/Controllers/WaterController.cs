using Microsoft.AspNetCore.Mvc;
using WaterProject.API.Data;

namespace WaterProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaterController : ControllerBase
    {
        private WaterDbContext _waterDbContext;
        
        public WaterController(WaterDbContext temp) => _waterDbContext = temp;

        [HttpGet("AllProjects")]
        public IActionResult GetProjects(int pageSize = 10, int pageNum = 1, [FromQuery] List<string>? projectTypes = null)
        {
            string favProjType = Request.Cookies["FavoriteProjectType"];
            Console.WriteLine("~~~~~~COOKIE~~~~~~\n" + favProjType);
            
            HttpContext.Response.Cookies.Append("FavoriteProjectType", "Protected String", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddMinutes(1),
            });

            var query = _waterDbContext.Projects.AsQueryable();

            if (projectTypes != null && projectTypes.Any())
            {
                query = query.Where(p => projectTypes.Contains(p.ProjectType));
            }
            
            var totalNumProjects = query.Count();
            
            var something = query
                .Skip((pageNum-1) * pageSize)
                .Take(pageSize)
                .ToList();

            var someObject = new
            {
                Projects = something,
                TotalNumProjects = totalNumProjects
            };

            return Ok(someObject);
        }

        [HttpGet("GetProjectTypes")]
        public IActionResult GetProjectTypes()
        {
            var projectTypes = _waterDbContext.Projects
                .Select(p => p.ProjectType)
                .Distinct()
                .ToList();

            return Ok(projectTypes);
        }

        [HttpPost("AddProject")]
        public IActionResult AddProject([FromBody] Project newProject)
        {
            _waterDbContext.Projects.Add(newProject);
            _waterDbContext.SaveChanges();
            return Ok(newProject);
        }

        [HttpPut("UpdateProject/{projectId}")]
        public IActionResult UpdateProject(int projectId, [FromBody] Project updatedProject)
        {
            var existingProject = _waterDbContext.Projects.Find(projectId);

            existingProject.ProjectName = updatedProject.ProjectName;
            existingProject.ProjectType = updatedProject.ProjectType;
            existingProject.ProjectRegionalProgram = updatedProject.ProjectRegionalProgram;
            existingProject.ProjectImpact = updatedProject.ProjectImpact;
            existingProject.ProjectPhase = updatedProject.ProjectPhase;
            existingProject.ProjectFunctionalityStatus = updatedProject.ProjectFunctionalityStatus;

            _waterDbContext.Projects.Update(existingProject);
            _waterDbContext.SaveChanges();

            return Ok(existingProject);
        }

        [HttpDelete("DeleteProject/{projectId}")]
        public IActionResult DeleteProject(int projectId)
        {
            var project = _waterDbContext.Projects.Find(projectId);

            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            _waterDbContext.Projects.Remove(project);
            _waterDbContext.SaveChanges();

            return NoContent();
        }
    }
}