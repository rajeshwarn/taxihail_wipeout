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

import com.google.android.m4b.maps.StreetViewPanorama;
import com.google.android.m4b.maps.StreetViewPanorama.OnStreetViewPanoramaCameraChangeListener;
import com.google.android.m4b.maps.StreetViewPanorama.OnStreetViewPanoramaChangeListener;
import com.google.android.m4b.maps.StreetViewPanorama.OnStreetViewPanoramaClickListener;
import com.google.android.m4b.maps.StreetViewPanoramaFragment;
import com.google.android.m4b.maps.model.LatLng;
import com.google.android.m4b.maps.model.StreetViewPanoramaCamera;
import com.google.android.m4b.maps.model.StreetViewPanoramaLocation;
import com.google.android.m4b.maps.model.StreetViewPanoramaOrientation;

import android.graphics.Point;
import android.os.Bundle;
import android.app.Activity;
import android.widget.TextView;

/**
 * This shows how to listen to some {@link StreetViewPanorama} events.
 */
public class StreetViewPanoramaEventsDemoActivity extends Activity
        implements OnStreetViewPanoramaChangeListener, OnStreetViewPanoramaCameraChangeListener,
        OnStreetViewPanoramaClickListener {

    // George St, Sydney
    private static final LatLng SYDNEY = new LatLng(-33.87365, 151.20689);

    private StreetViewPanorama mSvp;

    private TextView mPanoChangeTimesTextView;
    private TextView mPanoCameraChangeTextView;
    private TextView mPanoClickTextView;

    private int mPanoChangeTimes = 0;
    private int mPanoCameraChangeTimes = 0;
    private int mPanoClickTimes = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.street_view_panorama_events_demo);

        setUpStreetViewPanoramaIfNeeded(savedInstanceState);

        mPanoChangeTimesTextView = (TextView) findViewById(R.id.change_pano);
        mPanoCameraChangeTextView = (TextView) findViewById(R.id.change_camera);
        mPanoClickTextView = (TextView) findViewById(R.id.click_pano);
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
                mSvp.setOnStreetViewPanoramaChangeListener(this);
                mSvp.setOnStreetViewPanoramaCameraChangeListener(this);
                mSvp.setOnStreetViewPanoramaClickListener(this);
            }
        }
    }

    @Override
    public void onStreetViewPanoramaChange(StreetViewPanoramaLocation location) {
        if (location != null) {
            mPanoChangeTimesTextView.setText("Times panorama changed=" + ++mPanoChangeTimes);
        }
    }

    @Override
    public void onStreetViewPanoramaCameraChange(StreetViewPanoramaCamera camera) {
        mPanoCameraChangeTextView.setText("Times camera changed=" + ++mPanoCameraChangeTimes);
    }

    @Override
    public void onStreetViewPanoramaClick(StreetViewPanoramaOrientation orientation) {
        Point point = mSvp.orientationToPoint(orientation);
        if (point != null) {
            mPanoClickTextView.setText(
                "Times clicked=" + ++mPanoClickTimes + " :" + point.toString());
            mSvp.animateTo(new StreetViewPanoramaCamera.Builder().orientation(orientation)
                .zoom(mSvp.getPanoramaCamera().zoom).build(), 1000);
        }
    }
}
