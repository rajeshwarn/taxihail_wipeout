﻿using System;
using System.Collections.Generic;

namespace apcurium.MK.Common.Geography
{
    public static class GeographyHelper
    {
        /// <summary>
        /// Method that calculates polygon points of a circle.
        /// Adapted from: http://stackoverflow.com/questions/11688922/create-a-sqlgeography-polygon-circle-from-a-center-and-radius
        /// </summary>
        /// <param name="latitude">Origin latitude</param>
        /// <param name="longitude">Origin longitude</param>
        /// /// <param name="distanceFromCenter">Circle radius (in kilometers)</param>
        /// <param name="numberOfPoints">Number of circle points to generate</param>
        /// <returns>Returns a collection containing the specified number of points forming a closed circle</returns>
        public static IEnumerable<Tuple<double, double>> CirclePointsFromRadius(double latitude, double longitude, int distanceFromCenter, int numberOfPoints)
        {
            // All calculations done in radians
            // DO NOT use var. Explicit numerical type is important to avoid rounding errors

            const double earthRadius = 6371;
            int angleIncrement = 360 / (numberOfPoints - 1);

            double centerLatitude = (latitude * Math.PI) / 180;
            double centerLongitude = (longitude * Math.PI) / 180;
            double radius = distanceFromCenter / earthRadius;

            var circlePoints = new List<Tuple<double, double>>();

            for (var x = 0; x <= 360; x += angleIncrement)
            {
                // Excecuting magic; do not touch this, you poor soul.
                var angle = x * Math.PI / 180;
                var pointLatitude = Math.Asin(Math.Sin(centerLatitude) * Math.Cos(radius) + Math.Cos(centerLatitude) * Math.Sin(radius) * Math.Cos(angle));
                var pointLongitude = ((centerLongitude + Math.Atan2(Math.Sin(angle) * Math.Sin(radius) * Math.Cos(centerLatitude), Math.Cos(radius) - Math.Sin(centerLatitude) * Math.Sin(pointLatitude))) * 180) / Math.PI;

                circlePoints.Add(new Tuple<double, double>((pointLatitude * 180) / Math.PI, pointLongitude));
            }

            return circlePoints;
        }
    }
}
