## Initial Data Contract Thoughts
```
    registrant: {
        ...base_model,
        name: string,
        address: Address,
        email: string,
        birthday: date,
        competitionStatus: ENUM
        competition_status: spectator/participant,
        events: [Event],
        age_level_talent: Early/Senior,
        age_level_individual: Junior/Middle/Senior,
        church_id: uuid
    }
```

## Permissioning 
https://www.josephguadagno.net/2020/07/01/no-route-matches-the-supplied-values