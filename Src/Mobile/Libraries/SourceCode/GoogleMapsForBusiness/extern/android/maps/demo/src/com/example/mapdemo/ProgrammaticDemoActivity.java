/*
 * Copyright (C) 2012 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.example.mapdemo;

import com.google.android.m4b.maps.GoogleMap;
import com.google.android.m4b.maps.MapFragment;
import com.google.android.m4b.maps.model.LatLng;
import com.google.android.m4b.maps.model.MarkerOptions;

import android.os.Bundle;
import android.app.Activity;
import android.app.FragmentTransaction;

/**
 * Demonstrates how to instantiate a MapFragment programmatically and add a marker to it.
 */
public class ProgrammaticDemoActivity extends Activity {

    private static final String MAP_FRAGMENT_TAG = "map";
    private GoogleMap mMap;
    private MapFragment mMapFragment;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // It isn't possible to set a fragment's id programmatically so we set a tag instead and
        // search for it using that.
        mMapFragment = (MapFragment) getFragmentManager()
                .findFragmentByTag(MAP_FRAGMENT_TAG);

        // We only create a fragment if it doesn't already exist.
        if (mMapFragment == null) {
            // To programmatically add the map, we first create a MapFragment.
            mMapFragment = MapFragment.newInstance();

            // Then we add it using a FragmentTransaction.
            FragmentTransaction fragmentTransaction =
                    getFragmentManager().beginTransaction();
            fragmentTransaction.add(android.R.id.content, mMapFragment, MAP_FRAGMENT_TAG);
            fragmentTransaction.commit();
        }

        // We can't be guaranteed that the map is available because Google Play services might
        // not be available.
        setUpMapIfNeeded();
    }

    @Override
    protected void onResume() {
        super.onResume();

        // In case Google Play services has since become available.
        setUpMapIfNeeded();
    }

    private void setUpMapIfNeeded() {
        // Do a null check to confirm that we have not already instantiated the map.
        if (mMap == null) {
            // Try to obtain the map from the MapFragment.
            mMap = mMapFragment.getMap();
            // Check if we were successful in obtaining the map.
            if (mMap != null) {
                setUpMap();
            }
        }
    }

    private void setUpMap() {
        mMap.addMarker(new MarkerOptions().position(new LatLng(0, 0)).title("Marker"));
    }
}
