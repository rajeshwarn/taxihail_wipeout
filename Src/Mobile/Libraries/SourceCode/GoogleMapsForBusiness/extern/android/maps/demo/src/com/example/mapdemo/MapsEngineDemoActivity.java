// Copyright 2013 Google Inc. All Rights Reserved.

package com.example.mapdemo;

import com.google.android.m4b.maps.GoogleMap;
import com.google.android.m4b.maps.GoogleMap.OnMapClickListener;
import com.google.android.m4b.maps.GoogleMap.OnMapsEngineFeatureClickListener;
import com.google.android.m4b.maps.MapFragment;
import com.google.android.m4b.maps.model.LatLng;
import com.google.android.m4b.maps.model.MapsEngineFeature;
import com.google.android.m4b.maps.model.MapsEngineLayer;
import com.google.android.m4b.maps.model.MapsEngineLayerOptions;

import android.os.Bundle;
import android.app.Activity;
import android.util.Log;
import android.view.View;
import android.widget.CheckBox;

import java.util.List;

/**
 * Displays a maps engine layer on a map.
 */
public final class MapsEngineDemoActivity extends Activity implements
    OnMapsEngineFeatureClickListener, OnMapClickListener {

  private MapsEngineLayer sydney;
  private MapsEngineLayer urban;
  private MapsEngineLayer coastal;
  private MapsEngineLayer airports;
  private MapsEngineLayer cameras;

  @Override
  protected void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
    setContentView(R.layout.maps_engine_demo);

    GoogleMap map = (((MapFragment) getFragmentManager()
        .findFragmentById(R.id.map))).getMap();

    // map.setOnMapsEngineFeatureClickListener(this);
    // map.setOnMapClickListener(this);

    // This shows general regions of Sydney. It is a vector layer and demonstrates clickable
    // regions.
    sydney = map.addMapsEngineLayer(new MapsEngineLayerOptions().layerInMap(
        "16150640193109660958-06229419571706175761", "sydney").defaultUi(true));

    // This shows cities in the US. It is a vector layer and demonstrates clickable point features.
    // It is called us-cities-for-android-api-testing in M4B playground.
    urban = map.addMapsEngineLayer(new MapsEngineLayerOptions()
        .layerId("14182859561222861561-13622931834983879294"));

    // This shows US coastal satellite imagery. It is an imagery layer.
    coastal = map.addMapsEngineLayer(new MapsEngineLayerOptions()
        .layerId("10446176163891957399-12677872887550376890"));

    // This shows airports in the US (bent-styled-airportx010g)
    airports = map.addMapsEngineLayer(new MapsEngineLayerOptions()
        .layerId("14182859561222861561-12012127489080757244"));

    // This layer is a public layer which can only be loaded by MapID/Key/Version. It uses icons
    // as tap elements which have HTML descriptions containing tourist location web cams.
    cameras = map.addMapsEngineLayer(new MapsEngineLayerOptions().layerInMap(
        "10446176163891957399-09362451985983837383", "layer_00004"));
  }

  @Override
  public void onMapClick(LatLng point) {
    // dismissFeatureInfocard();
  }

  @Override
  public void onFeatureClick(List<MapsEngineFeature> features) {
    Log.i("MapsEngineActivity", "Feature clicked (" + features.size() + ")");
  }

  @Override
  public void onFeatureInformationReceived(List<MapsEngineFeature> features) {
    Log.i("MapsEngineActivity", "HTML received (" + features.size() + ")");
  }

  //
  // These callbacks are set in the XML layout file.
  //

  public void toggleSydney(View v) {
    sydney.setVisible(((CheckBox) v).isChecked());
  }

  public void toggleUrban(View v) {
    urban.setVisible(((CheckBox) v).isChecked());
  }

  public void toggleCoastal(View v) {
    coastal.setVisible(((CheckBox) v).isChecked());
  }

  public void toggleAirports(View v) {
    airports.setVisible(((CheckBox) v).isChecked());
  }

  public void toggleCameras(View v) {
    cameras.setVisible(((CheckBox) v).isChecked());
  }
}
