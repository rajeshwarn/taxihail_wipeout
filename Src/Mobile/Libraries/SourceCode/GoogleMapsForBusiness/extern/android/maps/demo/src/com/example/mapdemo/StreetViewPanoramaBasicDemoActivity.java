package com.example.mapdemo;

import com.google.android.m4b.maps.StreetViewPanorama;
import com.google.android.m4b.maps.StreetViewPanoramaFragment;
import com.google.android.m4b.maps.model.LatLng;

import android.os.Bundle;
import android.app.Activity;

/**
 * This shows how to create a simple activity with streetview
 */
public class StreetViewPanoramaBasicDemoActivity extends Activity {

    private StreetViewPanorama mSvp;

    // George St, Sydney
    private static final LatLng SYDNEY = new LatLng(-33.87365, 151.20689);

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.street_view_panorama_basic_demo);

        setUpStreetViewPanoramaIfNeeded(savedInstanceState);
    }

    private void setUpStreetViewPanoramaIfNeeded(Bundle savedInstanceState) {
        if (mSvp == null) {
            mSvp = ((StreetViewPanoramaFragment)
                getFragmentManager().findFragmentById(R.id.streetviewpanorama))
                    .getStreetViewPanorama();
            if (mSvp != null) {
                if (savedInstanceState == null) {
                    mSvp.setPosition(SYDNEY);
                }
            }
        }
    }
}
