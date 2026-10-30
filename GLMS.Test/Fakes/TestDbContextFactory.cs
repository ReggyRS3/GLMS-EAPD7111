using GLMS.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Test.Fakes
{
    public static class TestDbContextFactory
    {
        public static GlmsDbContext Create()
        {
            var options = new DbContextOptionsBuilder<GlmsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new GlmsDbContext(options);
        }
    }
}