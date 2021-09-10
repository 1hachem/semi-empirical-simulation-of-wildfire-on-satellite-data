# satellite-imagery-wildfire-simulation
Simulation of wild fire based on real world satlite imagery to estimate potential damage and to optimize risk management.

* extracting data from satelite imagery notebook [colab](https://colab.research.google.com/drive/1xwIUGick9HLaP-HN5vj4ibcZZvoyCalv?usp=sharing)

## vegetation grid 

a k-means classification to segment satelite image into 4 categories : soil, water, vegetation,urban area

<img src="https://user-images.githubusercontent.com/84399880/132776721-2fed4938-d6c9-44b1-825b-2e7a6452fd7c.png" alt="drawing" style="width:500px;"/>
<img src="https://user-images.githubusercontent.com/84399880/132776731-794ee175-22e2-42de-b169-df9106265b5c.png" alt="drawing" style="width:500px;"/>

## elevation grid

extract information about the elevation (m) of each point of land

<img src="https://user-images.githubusercontent.com/84399880/132777099-cd9e3ed4-ab01-47ba-898f-93bd11ced6a6.png" alt="drawing" style="width:500px;"/>
<img src="https://user-images.githubusercontent.com/84399880/132777202-25540eae-44ae-4cca-8888-dbba65549980.png" alt="drawing" style="width:500px;"/>


#wind and moisture data
we project the wind data and the moisture data on top of the elevation and vegetation grid and wrap all these features in a pd dataframe, example :

pixel | veg_class	| elevation | wind_u	| wind_v | moisture
----|---|---|---|---|---------
0 |	2 |	379 |	-0.13 |	1.56 |	0.873476
2199 | 2|	259|	0.25|	1.22|	0.819649
