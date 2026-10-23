# Contributing

Security changes require tests that exercise both allowed and denied paths.

- Keep authorization deny-by-default.
- Never add a tool execution path that bypasses ExecutionEngine.
- Preserve provenance through every transformation, serialization boundary, and control dependency.
- Keep argument contents out of exception messages and decision records.
- Add adversarial tests for malformed plans, untrusted inputs, and fail-closed behavior.
- Run dotnet build CapabilityGate.sln --configuration Release and dotnet test CapabilityGate.sln --configuration Release in an isolated runner.
- Do not install SDKs on the project owner's laptop.
