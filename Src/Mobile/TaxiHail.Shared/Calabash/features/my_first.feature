Feature: Automate screenshots for TaxiHail
  Tests should allow to 
  Move the map to certain position,
  To test the colors on the different views
  and to take screen shots

Scenario: TaxiHail Steps
  Given the app is running
  Then I take screenshots
  Then I change the address BETA
 