# Learning convention

Applies whenever the user asks a how/what/why/explain-style question. Assume they are new to this domain (audiology/device/clinical workflow terms need plain explanation, not just the code) but an experienced .NET/C#/WPF-MVVM developer otherwise — skip framework 101, but don't assume domain vocabulary.

For any problem or blocker specifically, always cover three things, each in simple terms first and then in technical terms:

```
1. What's blocking — simple terms, then technical terms
2. Why it's actually a blocker — simple terms, then technical terms
3. How it can be resolved — simple terms, then technical terms
4. Summary — concise enough to hand to a lead as-is, no jargon dump
```

For other explanations (not a blocker), ground the "why" in the actual architectural rule it stems from (cite `AccuSync Architecture.md` or the relevant HLD) rather than re-describing what the code does, and prefer a short concrete example (a real file:line, a real test) over an abstract description.
