using System.Web.Mvc;

namespace CerebelloWebRole.Code.Filters
{
    /// <summary>
    /// Marker filter used to indicate that it should be possible to alternate from one user to the other, using the same page parameters.
    /// </summary>
    public class CanAlternateUserAttribute : FilterAttribute
    {
    }
}
