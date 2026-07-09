# Climate

Adds regional climates and instruments to measure them to the game. This does not add visual clime effects.  

## Regional Climates

Each region will have a baseline climate. This climate will vary daily as well as change throughout the day. Storms will affect the climate also.  

## Instruments

All instruments can be bought in Gold Rock City, Dragon Cliffs, and Ft. Aestrin near where navigation equipment is sold.  

The thermometer and hygrometer values are based on the regional climates added to the game.  

The values read from the barometer are purely based on how close you are to the nearest storm. A value of 29.7 inHg means you are at the outer edges of a storm and will start to experience rain. A value of 27.65 inHg means that you are in the storm wall and should be experiencing very stormy weather. A value of 26 inHg means you are at the center of the storm.  

## For Other Mod Authors

The [WeatherService](https://github.com/bryon82/SailwindClimate/blob/main/Climate/Services/WeatherService.cs) class offers many different functions to retrieve climate related values if you would like to use any weather related data from this mod.  

I use prefab directory indexes 820 - 822.  

### Installation

If updating, remove Climate folders and/or Climate.dll files from previous installations.  
<br>
Extract the downloaded zip. Inside the extracted Climate-\<version\> folder copy the Climate folder and paste it into the Sailwind/BepInEx/Plugins folder.  

#### Consider supporting me 🤗

<a href='https://www.paypal.com/donate/?business=WKY25BB3TSH6E&no_recurring=0&item_name=Thank+you+for+your+support%21+I%27m+glad+you+are+enjoying+my+mods%21&currency_code=USD' target='_blank'><img src="https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif" border="0" alt="Donate with PayPal button" />
<a href='https://ko-fi.com/S6S11DDLMC' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://storage.ko-fi.com/cdn/kofi6.png?v=6' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>
