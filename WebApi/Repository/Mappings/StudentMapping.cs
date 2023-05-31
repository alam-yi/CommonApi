using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Model;

namespace Repository
{
    public class StudentMapping : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            // 定义主键
            builder.ToTable("student").HasKey(p => p.ID);
            // 定义字段Name
            builder.Property(p => p.Name).IsRequired().HasMaxLength(20);
            // 定义字段Address
            builder.Property(p => p.Age);
        }
    }
}
