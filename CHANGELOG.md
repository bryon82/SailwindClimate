# Changelog

All notable changes to this project will be documented in this file.

## [v1.1.0] - 2026-07-26

### Added
- Pressure cells, 6 of them will always be on the map. Baseline pressure is 29.7inHg and pressure cells will affect this. Pressure cells can be high or low with varying intensity, will move through the map, will have a lifespan of a few days, and will vary in their intensity depending on where they are in their lifespan and on how close you are to their center.
- Persistence of pressure cells when loading a save.
- Radiative cooling and low pressure cooling.
- Fog that occurs when temps drop close to the region's dew point.
- Clouds that occurs when humidty is high enough and pressure is low enough.
- Rain that occurs when humidty is high enough and pressure is low enough.

### Updated
- Functions in WeatherService by overloading them to get the current or current at player position values instead of having dedicated "Current" named functions.

## [v1.0.1] - 2026-07-20

### Added
- Pressure field in ShipItemBarometer for RadRefinements to read.

## [v1.0.0] - 2026-07-13

### Updated
- Temperature to be based off of latitude.

## [v0.2.0] - 2026-07-09

### Added
- Blending of major climate zones at the borders.
- GetCurrent versions of all functions in WeatherService.

### Updated
- Internal temp calculations to use Celsius instead of Fahrenheit.

## [v0.1.0] - 2026-07-07

### Added
- Barometer, thermometer, and hygrometer.
- Climate zones with temperature and dew point for the different regions of the game.
- A public WeatherService class which exposes many functions for other mods to retrieve various weather values.