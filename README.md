# CapabilityGate

CapabilityGate is a .NET reference monitor for agent tool execution. It combines fixed plans, explicit tool capabilities, and provenance-tagged values so an application can reject tool calls whose arguments or control flow depend on sources the host has not allowed.

This is an early security library, not a production certification and not a universal prompt-injection solution. Use it only behind a tested application boundary and conduct an independent security review before deploying it with consequential tools.

## Design

- Build a plan once, validate its size, and execute its steps in order.
- Register allowed tool capabilities explicitly. Unknown tools and missing capabilities are denied.
- Track provenance on values and control dependencies. Transformations preserve source labels, and policies declare allowed sources per argument.
- Keep untrusted text handling behind an IQuarantinedModel interface that has no tool-execution API. Quarantined output remains tainted.
- Pass tool calls to executors only through AuthorizedToolCall; its constructor is internal.
- Return decision records without argument values to avoid copying sensitive inputs into audit output.
- Load strict JSON policy documents through `CapabilityPolicy.FromJson`.

The host is the root of trust. It must mark source provenance correctly, prevent untrusted content from entering the planner as trusted intent, and ensure all consequential tools execute through this boundary. The current API expects the host to supply a fixed plan; it does not connect to an LLM or generate plans.

This package does not sandbox a model process, inspect network traffic, roll back side effects, or protect a compromised host or tool. It provides an enforcement component, not an end-to-end guarantee.

## Research basis

CaMeL separates a planner from a quarantined model, propagates data and control dependencies, and gates tool calls with capability policies. Its reported security and utility numbers are specific to the paper's versions, models, and evaluated tasks. The initial CaMeL paper reported 67% utility; the current version reports 77% in its abstract. Neither result proves that this library or all agent workflows are secure. See [CaMeL v1](https://arxiv.org/pdf/2503.18813v1), [CaMeL current version](https://arxiv.org/abs/2503.18813), and [Design Patterns for Securing LLM Agents against Prompt Injections](https://arxiv.org/abs/2506.08837).

## Install and use

The API is under development. Build from source with the .NET 10 SDK. CI installs the SDK on GitHub-hosted runners; this project does not require a local SDK installation.

The first release includes the .NET library only. A TypeScript wrapper can follow after the .NET contract and threat model stabilize.

## Optional Common Lisp policy compiler

`tools/policy-lisp/compile.lisp` compiles a restricted S-expression policy file to the strict JSON format accepted by `CapabilityPolicy.FromJson`:

```sh
sbcl --script tools/policy-lisp/compile.lisp tools/policy-lisp/examples/minimal.sexp > policy.json
```

Policy files are parsed as data by a small tokenizer and parser. The compiler does not call the Lisp reader or evaluate input forms, and rejects unrecognized forms and trust labels. The generated file still requires review and the .NET reference monitor remains the enforcement point. CI checks valid output and rejects an invalid example. SBCL is installed only on the GitHub-hosted runner.

## Security reporting

Use private vulnerability reporting through GitHub Security Advisories. Never submit live secrets, private user data, or real agent traces to public issues.

Read SECURITY.md, CONTRIBUTING.md, and THREAT-MODEL.md before integrating the package.
