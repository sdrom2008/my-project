using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Domain.ValueObjects
{
    public record OptimizedImage(
        string OriginalUrl,
        string OptimizedUrl,
        string Description,          // e.g. "主图-场景合成-白领穿搭"
        ImageType Type,              // Main, Detail, APlus 等
        int QualityScore             // AGI自评 0-100
    );
    public enum ImageType
    {
        Product,
        Avatar,
        Banner,
        Other
    }
}
