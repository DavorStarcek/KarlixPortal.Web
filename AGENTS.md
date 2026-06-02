# KarlixPortal.Web - Agent Instructions

This repository contains the Karlix Portal MVC client.

## Responsibility

- Main portal client
- OIDC login/logout through KarlixID
- Entry point for Karlix modules
- Displays authenticated user, claims, tenant and navigation

## Rules

- Do not change OIDC authentication flow unless explicitly requested.
- Do not move business authorization into Portal.
- Preserve existing MVC patterns.
- Preserve existing tenant/claim display behavior.
- Do not connect to production services.
- Do not modify secrets unless explicitly requested.
- Do not edit sibling repositories unless explicitly requested.
- Do not modify bin, obj, .vs, *.user, *.suo files.

## Build

Default build command:

```bash
dotnet build .\KarlixPortal.Web.csproj

Workflow

Before coding:

Inspect existing implementation.
Identify affected controller, view, service or config.
Propose a short plan.
Keep scope narrow.

After coding:

Run build if allowed.
Summarize changed files.
Report build result.
Mention risks and next steps.

Commit:

```bash
git add AGENTS.md
git commit -m "docs: add agent instructions"
git push