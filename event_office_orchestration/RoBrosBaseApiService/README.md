# RoBros Base Api
RoBros Base Api is foundational entity in a domain driven design system looking to separate entities by nouns, verbs, and queries.  The base api will use optimistic locking by versioning each entity.  Nouns will be the only entities that are concerned with this base api and they will inherit the ABC class and extend it to meet their needs.  

## Separation
While fleshing out the full scale of the system this will be a mono repo with the services nested underneath.  