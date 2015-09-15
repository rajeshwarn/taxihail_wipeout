﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MK.Booking.MapDataProvider.Foursquare
{
    /// <summary>
    /// https://github.com/TICLAB/SharpSquare/tree/master/Entities
    /// </summary>
    public class FoursquareVenuesResponse<T>
    {
		const int HTTPOKResponse = 200;

		public Meta Meta { get; set; }
        public T Response { get; set; }

		public bool IsValid()
		{
			return Meta == null || (Meta != null && Meta.Code == HTTPOKResponse);
		}
    }

	public class Meta
	{
		public int Code { get; set; }

		public string RequestId { get; set; }

		public string ErrorType { get; set; }

		public string ErrorDetail { get; set; }
	}

	public class SuggestedFilter
	{
		public string Name { get; set; }

		public string Key { get; set; }
	}

	public class SuggestedFilters
	{
		public string Header { get; set; }

		public SuggestedFilter[] Filters { get; set; }

	}

	public class ReasonItem
	{
		public string Summary { get; set; }

		public string Type { get; set; }

		public string ReasonName { get; set; }

	}

	public class Reasons
	{
		public int Count { get; set; }

		public ReasonItem[] Items { get; set; }
	}

	public class GroupItem
	{
		public Reasons Reasons { get; set; }

		public Venue Venue { get; set; }
	}

	public class Group
	{
		public string Type { get; set; }

		public string Name { get; set; }

		public GroupItem[] Items { get; set; }
	}

	public class ExploreResponse
	{
		public SuggestedFilters SuggestedFilters { get; set; }

		public string HeaderLocation { get; set; }

		public string HeaderFullLocation { get; set; }

		public string HeaderLocationGranularity { get; set; }

		public int TotalResults { get; set; }

		public Group[] Groups { get; set; }
	}


    public class SearchResponse
    {
        public Venue[] Venues { get; set; }
    }

    public class VenueResponse
    {
        public Venue Venue { get; set; }
    }

	public class Venue
	{
	    public Venue()
	    {
	        categories = new List<Category>();
	    }

		///
		///  <summary>
		/// A unique string identifier for this venue.
		/// </summary>
		public string id
		{
			get;
			set;
		}

		/// <summary>
		/// The best known name for this venue.
		/// </summary>
		public string name
		{
			get;
			set;
		}

        public List<Category> categories
        {
            get;
            set;
        }

		/// <summary>
		///  An object containing none, some, or all of address (street address), crossStreet, city, state, postalCode, country, lat, lng, and distance. All fields are strings, except for lat, lng, and distance. Distance is measured in meters. 
		///  Some venues have their locations intentionally hidden for privacy reasons (such as private residences). If this is the case, the parameter isFuzzed will be set to true, and the lat/lng parameters will have reduced precision. 
		/// </summary>
		public Location location
		{
			get;
			set;
		}

		/// <summary>
		/// Boolean indicating whether the owner of this business has claimed it and verified the information.
		/// </summary>
		public bool verified
		{
			get;
			set;
		}

		public bool restricted
		{
			get;
			set;
		}

		/// <summary>
		///  URL of the venue's website, typically provided by the venue manager. 
		/// </summary>
		public string url
		{
			get;
			set;
		}


		/// <summary>
		/// The manager's internal identifier for the venue. 
		/// </summary>
		public string storeId
		{
			get;
			set;
		}

		/// <summary>
		/// Optional. Description of the venue provided by venue owner. 
		/// </summary>
		public string description
		{
			get;
			set;
		}

		/// <summary>
		/// Seconds since epoch when the venue was created.  
		/// </summary>
		public Int64 createdAt
		{
			get;
			set;
		}



		/// <summary>
		/// An array of string tags applied to this venue.
		/// </summary>
		public List<string> tags
		{
			get;
			set;
		}


		/// <summary>
		/// A short URL for this venue, e.g. http://4sq.com/Ab123D
		/// </summary>
		public string shortUrl
		{
			get;
			set;
		}

		/// <summary>
		/// The canonical URL for this venue, e.g. https://foursquare.com/v/foursquare-hq/4ab7e57cf964a5205f7b20e3
		/// </summary>
		public string canonicalUrl
		{
			get;
			set;
		}


		/// <summary>
		/// Indicates if the current user has liked this venue.  
		/// </summary>
		public bool like
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates if the current user has disliked this venue. 
		/// </summary>
		public bool dislike
		{
			get;
			set;
		}

		/// <summary>
		/// Time zone, e.g. America/New_York 
		/// </summary>
		public string timeZone
		{
			get;
			set;
		}

		public double rating
		{
			get;
			set;
		}

		/// <summary>
		///  Present if and only if the current user has at least one assigned role for this venue. The value is a list of all of the current user's assigned roles for this venue. Possible values for each element of the list are manager and employee. Subject to change as additional roles may be defined.
		/// </summary>
		public List<string> roles
		{
			get;
			set;
		}


	}

	public class Location
	{
		public string address
		{
			get;
			set;
		}

		public string crossStreet
		{
			get;
			set;
		}

		public string city
		{
			get;
			set;
		}

		public string state
		{
			get;
			set;
		}

		public string postalCode
		{
			get;
			set;
		}

		public string country
		{
			get;
			set;
		}

		public double lat
		{
			get;
			set;
		}

		public double lng
		{
			get;
			set;
		}

		public int distance
		{
			get;
			set;
		}

		public string cc
		{
			get;
			set;
		}
	}

    public class Category
    {
        /// <summary>
        /// A unique identifier for this category.
        /// </summary>
        public string id
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the category.
        /// </summary>
        public string name
        {
            get;
            set;
        }


        /// <summary>
        /// Pluralized version of the category name.
        /// </summary>
        public string pluralName
        {
            get;
            set;
        }


        /// <summary>
        /// Shorter version of the category name.
        /// </summary>
        public string shortName
        {
            get;
            set;
        }


        
        /// <summary>
        /// If this is the primary category for parent venue object.
        /// </summary>
        public List<string> parents
        {
            get;
            set;
        }


        /// <summary>
        /// If this is the primary category for parent venue object.
        /// </summary>
        public bool primary
        {
            get;
            set;
        }


        /// <summary>
        /// (only present in venues/categories response). A list of categories that are descendants of this category in the hierarchy.
        /// </summary>
        public List<Category> categories
        {
            get;
            set;
        }
    }


}
