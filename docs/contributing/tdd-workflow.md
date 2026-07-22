# TDD Workflow and Contribution Guide

## Core Rule
Every business or cross-cutting change follows Red/Green/Refactor.

## Workflow
1. Red: write failing tests describing expected behavior.
2. Green: implement the minimal code to pass.
3. Refactor: improve code clarity and maintain boundaries without changing behavior.

## Mandatory Checks Before Merge
1. `Result`/`Result<T>` is used for expected failures.
2. Module boundaries are preserved (no cross-module persistence coupling).
3. Unit tests cover new behavior and edge cases.
4. Documentation in `docs/` is updated with architectural or rule changes.

## Sub-phase 0.1 Focus
1. Shared kernel invariants.
2. Pipeline behavior reliability.
3. PWA shell readiness.