# Threat model

## Protected property

The gate aims to prevent a tool from executing unless the host explicitly granted its capability and the call's argument and control-flow provenance satisfy that tool's policy.

## In scope

- Indirect prompt injection carried in retrieved documents or tool results.
- Tainted model output used as a tool argument or to select a tool.
- Missing tool registrations or capabilities.
- Malformed and oversized static plans.

## Assumptions

- The host application and its policy configuration are trusted.
- The host labels sources correctly and does not relabel untrusted data as trusted.
- Every consequential tool invocation passes through ExecutionEngine.
- Tool implementations enforce their own authentication, input validation, and side-effect constraints.

## Out of scope

- A compromised host, runtime, model provider, or tool implementation.
- Direct malicious user instructions that conflict with the application's policy.
- Semantic misinformation that does not attempt to cause unauthorized actions.
- Covert channels, denial of service, rollback, and atomicity across external side effects.
- A guarantee that an LLM cannot produce unsafe content.

## Limitations

The library cannot inspect hidden provider state, model internals, network traffic, or an executor's behavior. The quarantine interface constrains the library API but does not create an operating-system sandbox. A wrapper or detector alone is not a security boundary.
