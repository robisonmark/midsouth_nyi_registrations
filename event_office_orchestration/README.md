# Event Office Orchestration
Momentum is a Midsouth NYI event which allows student to compete in a number of competitions, showcasing their talents and abilities.  The driving force behind this event is to help students understand that these talents were given to them by God and in return He asks us to use them for Him.  As the students compete they receive feedback and coaching that can help them improve on these talents, or even discover more talents that they didn't know they had.  Each of these competition is then assigned a point level for 1st, 2nd, 3rd place and participation. At the end of the event medals (or trophies) are awarded to acknowledge the students efforts. The medals (or trophies) are given to 1st, 2nd, 3rd, and in team events All-Stars.  The crowning achievement is a full church "grand" trophy acknowledging the combined talents of all the students from that church. Momentum mirrors TNT @ TNU, a field event bringing together students from across the south east of the United States. 

## Purpose
The purpose of this app is to help offset the work of registering/grouping students and teams, recording scores, tallying the scores, creating the presentation of winners, and keeping track of All-Stars. The hope is that at the end of this build we can provide a system to be used as part of TNT @ TNU.

## Language Used
- C# .Net
- Docker
- Kubernetes

## Code Organization
To assist in creating a scalable system the Event Office App will be separated by Domains. Each domain contains a "Business Object", in some places called Nouns or Entities and it will contain applicable business logic, called Verbs or Applications. An easy way to think of a entity is what is the smallest business object that can be grouped together.

### Base Layer
In an attempt to keep repository sprawl to a minimum we are setting up this app to have a root level orchestration layer.  When you need multiple containers to run you can used this base orchestration layer by (insert instructions here).  __one minor concern is independent scaling as part of the larger monorepo vs micro-repos__

### Sub-Directories
Each Sub-directory off this root layer is a Domain Service.
__of special note: EntityServices will share common properties that are outlined in the RoBrosBaseDomainService directory.__


### TODO
1. Build Orchestration Layer
2. Build Inheritance for RoBrosBaseDomainService (rework if necessary, think about NuGet packages.  this applies to the above)
3. Build RoBrosAddressService
4. Build RoBrosEventService
5. Build RoBrosRegistrantService
6. Build RoBrosTeamService
7. Build RoBrosScoreService
8. Build UI - Task out
9. Build RoBrosPresentationService
