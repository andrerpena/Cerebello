using System;
using System.Collections.Generic;

namespace CerebelloWebRole.Code
{
    public class AddressHelper
    {
        public static string GetAddressText(string street, string complement, string neighborhood, string city, string stateProvince, string CEP)
        {
            List<string> adressComponents = new List<string>();
            
            if (!String.IsNullOrEmpty(street))
                adressComponents.Add(street);

            if (!String.IsNullOrEmpty(complement))
                adressComponents.Add(complement);

            if (!String.IsNullOrEmpty(neighborhood))
                adressComponents.Add(neighborhood);

            if (!String.IsNullOrEmpty(city))
                adressComponents.Add(city);

            if (!String.IsNullOrEmpty(stateProvince))
                adressComponents.Add(stateProvince);

            if (!String.IsNullOrEmpty(CEP))
                adressComponents.Add(CEP);

            return string.Join(", ", adressComponents.ToArray());
        }
    }
}
