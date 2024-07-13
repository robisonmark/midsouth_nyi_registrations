## Initial Data Contract Thoughts
```
    scoring_model: {
        RubricModel: "Rubric Model",
        SingleEliminationBracket: "Single Elimination Bracket",
        DoubleEliminationBracket: "Double Elimination Bracket",
        RoundRobin: "Round Robin"
    }
    scoring: {
        ...base_model,
        scoring_model: ENUM<scoring_model>,
        
    }
```