# RoBros Presentation Service
This would be an Application Service that would aggregate scores, gathering finishing positions for each event with options to remove open event categories, and return a slide show (PowerPoint or ProPresenter).  One of the key biggest wins would be to add automatic transitions, that bring in places from 3rd to first.

## Thoughts on structure
As I write this I think that this would need to be a factory pattern.  Basic operations that change based on the product desired.  Each desired product would have its business logic contained with it.  Then with the correct options select it will out put in the desired format.