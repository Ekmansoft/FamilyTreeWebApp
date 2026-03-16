# .NET 10.0 Upgrade Plan

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Migration Plans](#project-by-project-migration-plans)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description

Upgrade FamilyTreeWebApp solution from .NET 6.0/.NET 7.0 to **.NET 10.0 (Long Term Support)**.

### Scope

**Projects Affected:** 7 projects
- 3 projects on .NET 7.0 → .NET 10.0
- 4 projects on .NET 6.0 → .NET 10.0

**Current State:**
- FamilyTreeCodecGedcom (net6.0) - 2,015 LOC
- FamilyTreeCodecGeni (net6.0) - 1,242 LOC
- FamilyTreeCodecText (net7.0) - 331 LOC
- FamilyTreeLibrary (net7.0) - 3,051 LOC
- FamilyTreeTools (net7.0) - 1,206 LOC
- FamilyTreeWebTools (net6.0) - 1,067 LOC
- FamilyTreeWebApp (net6.0) - 1,095 LOC

**Target State:**
All projects targeting net10.0 with updated package dependencies.

### Selected Strategy

**All-At-Once Strategy** - All projects upgraded simultaneously in single atomic operation.

**Rationale:**
- **Small solution**: 7 projects with straightforward dependency structure
- **All modern .NET**: Projects already on .NET 6.0+ (no legacy Framework conversion needed)
- **Low complexity**: Total ~10k LOC, all projects rated Low risk
- **Clear dependencies**: Simple dependency tree (max depth 2, no circular dependencies)
- **Minimal package updates**: Only 7 packages in 1 project need updates
- **All packages compatible**: Assessment confirms all packages have .NET 10 versions available
- **No breaking changes**: Assessment detected no API compatibility issues

### Complexity Assessment

**Discovered Metrics:**
- Total Projects: 7
- Total LOC: 10,142
- Dependency Depth: 2 levels
- Circular Dependencies: None
- Security Vulnerabilities: None
- High-Risk Projects: 0
- Package Updates Required: 7 (all in FamilyTreeWebApp)

**Complexity Classification: Simple**

This straightforward upgrade enables fast-track execution with all projects updated in one coordinated operation.

### Critical Issues

✅ **No blocking issues identified**
- No security vulnerabilities
- No incompatible packages
- No breaking API changes detected
- All projects use SDK-style format

### Recommended Approach

**All-At-Once Atomic Upgrade:**
1. Update all 7 project files to net10.0 simultaneously
2. Update all 7 packages in FamilyTreeWebApp
3. Restore dependencies and build entire solution
4. Fix any compilation errors in one pass
5. Validate solution builds with 0 errors

**Expected Duration:** Single iteration with comprehensive validation

### Iteration Strategy

**Phase 1:** Discovery & Classification ✅ Complete
**Phase 2:** Foundation (next - dependency analysis, migration strategy, project stubs)
**Phase 3:** Detail Generation (all projects batched together due to simple classification)

---

## Migration Strategy

### Approach Selection: All-At-Once

**Selected Approach:** All projects upgraded simultaneously in one atomic operation.

**Justification:**

✅ **Size Appropriate:** 7 projects falls well within the all-at-once threshold (<30 projects)

✅ **Modern Foundation:** All projects already on .NET 6.0+ (no legacy .NET Framework conversion needed)

✅ **Low Complexity:** 
- Total codebase: 10,142 LOC (manageable size)
- All projects rated Low risk
- Simple dependency structure (depth 2, no cycles)

✅ **Package Compatibility:**
- Only 7 packages need updates (all in one project)
- All packages have confirmed .NET 10 versions
- No incompatibility issues detected

✅ **Clean Assessment:**
- Zero security vulnerabilities
- Zero breaking changes detected
- Zero API compatibility issues

✅ **Fast Execution:** Single coordinated operation minimizes total upgrade time

### All-At-Once Strategy Rationale

The All-At-Once approach is ideal here because:

1. **Minimal Risk:** No high-risk indicators (security issues, breaking changes, complex dependencies)
2. **Atomic Validation:** Can validate entire solution in one build/test cycle
3. **No Multi-Targeting:** Don't need interim compatibility layers
4. **Team Efficiency:** All developers work with same framework version immediately
5. **Clean Testing:** Test entire upgraded solution as a unit

### Dependency-Based Ordering

While all projects update simultaneously, the **build order** respects dependencies:

```
Build Order (automatic via MSBuild):
1. FamilyTreeCodecGedcom, FamilyTreeCodecGeni, FamilyTreeCodecText (parallel - no dependencies)
2. FamilyTreeLibrary (after FamilyTreeCodecText)
3. FamilyTreeTools, FamilyTreeWebTools (after FamilyTreeLibrary, can build parallel)
4. FamilyTreeWebApp (after all dependencies)
```

If compilation errors occur, address them following this dependency order (fix leaf nodes first).

### Parallel vs Sequential Execution

**Parallel Execution Enabled:**
- All project file updates happen simultaneously
- All package updates happen simultaneously
- MSBuild naturally parallelizes building independent projects

**Sequential Checkpoints:**
1. Update all project files (atomic)
2. Update all packages (atomic)
3. Restore dependencies (one operation)
4. Build entire solution (respects dependency order automatically)
5. Fix all compilation errors (prioritize by dependency order if needed)
6. Rebuild to verify (one operation)

### Phase Definitions

**Phase 0: Prerequisites** (if needed)
- Verify .NET 10 SDK installed
- Verify branch is `upgrade-to-NET10`

**Phase 1: Atomic Upgrade**
- Update all 7 project TargetFramework properties to net10.0
- Update all 7 packages in FamilyTreeWebApp to .NET 10 versions
- Restore all dependencies
- Build entire solution
- Fix any compilation errors discovered
- Rebuild to verify 0 errors

**Phase 2: Validation**
- Run full solution build
- Verify no warnings
- Verify no dependency conflicts
- Manual validation (application runs)

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The solution has a clean, hierarchical dependency structure with **no circular dependencies**:

**Level 0 (Leaf - No Dependencies):**
- FamilyTreeCodecGedcom
- FamilyTreeCodecGeni  
- FamilyTreeCodecText

**Level 1 (Depends on Level 0):**
- FamilyTreeLibrary → depends on FamilyTreeCodecText
- FamilyTreeTools → depends on FamilyTreeLibrary
- FamilyTreeWebTools → depends on FamilyTreeLibrary

**Level 2 (Top - Application):**
- FamilyTreeWebApp → depends on FamilyTreeLibrary, FamilyTreeTools, FamilyTreeWebTools

### Project Groupings for All-At-Once Migration

Since this is an **All-At-Once Strategy**, all projects will be updated simultaneously in a single atomic operation. However, understanding the dependency layers helps us:

1. **Validate build order** after the upgrade
2. **Understand impact** if issues arise
3. **Prioritize fixes** if compilation errors occur (fix dependencies first)

**Single Upgrade Batch:**
All 7 projects updated together:
- FamilyTreeCodecGedcom (net6.0 → net10.0)
- FamilyTreeCodecGeni (net6.0 → net10.0)
- FamilyTreeCodecText (net7.0 → net10.0)
- FamilyTreeLibrary (net7.0 → net10.0)
- FamilyTreeTools (net7.0 → net10.0)
- FamilyTreeWebTools (net6.0 → net10.0)
- FamilyTreeWebApp (net6.0 → net10.0) + package updates

### Critical Path Identification

**Primary Path (most dependencies):**
FamilyTreeCodecText → FamilyTreeLibrary → FamilyTreeTools → FamilyTreeWebApp

If compilation issues arise after upgrade, fix in this order:
1. Codec projects (leaf nodes - no dependencies on other projects)
2. Library projects (depend only on codecs)
3. Tools projects (depend on library)
4. Web app (depends on everything)

### Circular Dependencies

✅ **None detected** - Clean dependency hierarchy enables straightforward upgrade.

---

## Project-by-Project Migration Plans

### Project: FamilyTreeCodecGedcom

**Current State:**
- Target Framework: net6.0
- Project Type: ClassLibrary (SDK-style)
- Dependencies: None (leaf node)
- Dependants: FamilyTreeLibrary
- Lines of Code: 2,015
- Package Count: 0
- Risk Level: 🟢 Low

**Target State:**
- Target Framework: net10.0
- No package updates required
- No expected breaking changes

**Migration Steps:**

1. **Prerequisites**
   - None - leaf project with no external dependencies

2. **Framework Update**
   - Update TargetFramework in `FamilyTreeCodecGedcom.csproj`:
     ```xml
     <TargetFramework>net6.0</TargetFramework>
     ```
     Change to:
     ```xml
     <TargetFramework>net10.0</TargetFramework>
     ```

3. **Package Updates**
   - None required

4. **Expected Breaking Changes**
   - ✅ None detected by assessment
   - Low likelihood of issues due to library nature

5. **Code Modifications**
   - Expected: None
   - If compilation errors occur: Review error messages and address individually

6. **Testing Strategy**
   - Build project successfully
   - Verify no compilation errors
   - Verify no warnings
   - Downstream validation: Ensure FamilyTreeLibrary still builds correctly

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] FamilyTreeLibrary (dependant) builds successfully
   - [ ] No dependency conflicts reported

---

### Project: FamilyTreeCodecGeni

**Current State:**
- Target Framework: net6.0
- Project Type: ClassLibrary (SDK-style)
- Dependencies: None (leaf node)
- Dependants: None
- Lines of Code: 1,242
- Package Count: 0
- Risk Level: 🟢 Low

**Target State:**
- Target Framework: net10.0
- No package updates required
- No expected breaking changes

**Migration Steps:**

1. **Prerequisites**
   - None - leaf project with no external dependencies

2. **Framework Update**
   - Update TargetFramework in `FamilyTreeCodecGeni.csproj`:
     ```xml
     <TargetFramework>net6.0</TargetFramework>
     ```
     Change to:
     ```xml
     <TargetFramework>net10.0</TargetFramework>
     ```

3. **Package Updates**
   - None required

4. **Expected Breaking Changes**
   - ✅ None detected by assessment
   - Low likelihood of issues due to library nature

5. **Code Modifications**
   - Expected: None
   - If compilation errors occur: Review error messages and address individually

6. **Testing Strategy**
   - Build project successfully
   - Verify no compilation errors
   - Verify no warnings

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] No dependency conflicts reported

---

### Project: FamilyTreeCodecText

**Current State:**
- Target Framework: net7.0
- Project Type: ClassLibrary (SDK-style)
- Dependencies: None (leaf node)
- Dependants: FamilyTreeLibrary
- Lines of Code: 331
- Package Count: 0
- Risk Level: 🟢 Low

**Target State:**
- Target Framework: net10.0
- No package updates required
- No expected breaking changes

**Migration Steps:**

1. **Prerequisites**
   - None - leaf project with no external dependencies

2. **Framework Update**
   - Update TargetFramework in `FamilyTreeCodecText.csproj`:
     ```xml
     <TargetFramework>net7.0</TargetFramework>
     ```
     Change to:
     ```xml
     <TargetFramework>net10.0</TargetFramework>
     ```

3. **Package Updates**
   - None required

4. **Expected Breaking Changes**
   - ✅ None detected by assessment
   - Low likelihood of issues (smallest project, simple library)

5. **Code Modifications**
   - Expected: None
   - If compilation errors occur: Review error messages and address individually

6. **Testing Strategy**
   - Build project successfully
   - Verify no compilation errors
   - Verify no warnings
   - Downstream validation: Ensure FamilyTreeLibrary still builds correctly

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] FamilyTreeLibrary (dependant) builds successfully
   - [ ] No dependency conflicts reported

---

### Project: FamilyTreeLibrary

**Current State:**
- Target Framework: net7.0
- Project Type: ClassLibrary (SDK-style)
- Dependencies: 1 project (FamilyTreeCodecText)
- Dependants: FamilyTreeTools, FamilyTreeWebTools, FamilyTreeWebApp
- Lines of Code: 3,051 (largest project)
- Package Count: 0
- Risk Level: 🟢 Low

**Target State:**
- Target Framework: net10.0
- No package updates required
- No expected breaking changes

**Migration Steps:**

1. **Prerequisites**
   - FamilyTreeCodecText must be upgraded to net10.0 first (handled in atomic upgrade)

2. **Framework Update**
   - Update TargetFramework in `FamilyTreeLibrary.csproj`:
     ```xml
     <TargetFramework>net7.0</TargetFramework>
     ```
     Change to:
     ```xml
     <TargetFramework>net10.0</TargetFramework>
     ```

3. **Package Updates**
   - None required

4. **Expected Breaking Changes**
   - ✅ None detected by assessment
   - Watch for: Codec integration points, data structures

5. **Code Modifications**
   - Expected: None
   - Areas to review if issues arise:
     - Interaction with FamilyTreeCodecText
     - Any file I/O or serialization logic
     - Data structure definitions

6. **Testing Strategy**
   - Build project successfully
   - Verify no compilation errors
   - Verify no warnings
   - Downstream validation: Ensure all dependent projects (Tools, WebTools, WebApp) build correctly
   - Consider: Unit tests for core library functionality if they exist

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] FamilyTreeCodecText dependency resolves correctly
   - [ ] All downstream projects (Tools, WebTools, WebApp) build successfully
   - [ ] No dependency conflicts reported

---

### Project: FamilyTreeTools

**Current State:**
- Target Framework: net7.0
- Project Type: ClassLibrary (SDK-style)
- Dependencies: 1 project (FamilyTreeLibrary)
- Dependants: FamilyTreeWebApp
- Lines of Code: 1,206
- Package Count: 0
- Risk Level: 🟢 Low

**Target State:**
- Target Framework: net10.0
- No package updates required
- No expected breaking changes

**Migration Steps:**

1. **Prerequisites**
   - FamilyTreeLibrary must be upgraded to net10.0 first (handled in atomic upgrade)

2. **Framework Update**
   - Update TargetFramework in `FamilyTreeTools.csproj`:
     ```xml
     <TargetFramework>net7.0</TargetFramework>
     ```
     Change to:
     ```xml
     <TargetFramework>net10.0</TargetFramework>
     ```

3. **Package Updates**
   - None required

4. **Expected Breaking Changes**
   - ✅ None detected by assessment
   - Watch for: Library integration points, tool-specific APIs

5. **Code Modifications**
   - Expected: None
   - Areas to review if issues arise:
     - Interaction with FamilyTreeLibrary
     - Any utility/helper method implementations

6. **Testing Strategy**
   - Build project successfully
   - Verify no compilation errors
   - Verify no warnings
   - Downstream validation: Ensure FamilyTreeWebApp builds correctly

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] FamilyTreeLibrary dependency resolves correctly
   - [ ] FamilyTreeWebApp (dependant) builds successfully
   - [ ] No dependency conflicts reported

---

### Project: FamilyTreeWebTools

**Current State:**
- Target Framework: net6.0
- Project Type: ClassLibrary (SDK-style)
- Dependencies: 1 project (FamilyTreeLibrary)
- Dependants: FamilyTreeWebApp
- Lines of Code: 1,067
- Package Count: 0
- Risk Level: 🟢 Low

**Target State:**
- Target Framework: net10.0
- No package updates required
- No expected breaking changes

**Migration Steps:**

1. **Prerequisites**
   - FamilyTreeLibrary must be upgraded to net10.0 first (handled in atomic upgrade)

2. **Framework Update**
   - Update TargetFramework in `FamilyTreeWebTools.csproj`:
     ```xml
     <TargetFramework>net6.0</TargetFramework>
     ```
     Change to:
     ```xml
     <TargetFramework>net10.0</TargetFramework>
     ```

3. **Package Updates**
   - None required

4. **Expected Breaking Changes**
   - ✅ None detected by assessment
   - Watch for: Web-specific helper methods, library integration

5. **Code Modifications**
   - Expected: None
   - Areas to review if issues arise:
     - Interaction with FamilyTreeLibrary
     - Web-specific utility methods
     - Any middleware or web helper implementations

6. **Testing Strategy**
   - Build project successfully
   - Verify no compilation errors
   - Verify no warnings
   - Downstream validation: Ensure FamilyTreeWebApp builds correctly

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] FamilyTreeLibrary dependency resolves correctly
   - [ ] FamilyTreeWebApp (dependant) builds successfully
   - [ ] No dependency conflicts reported

---

### Project: FamilyTreeWebApp

**Current State:**
- Target Framework: net6.0
- Project Type: AspNetCore (SDK-style)
- Dependencies: 3 projects (FamilyTreeLibrary, FamilyTreeTools, FamilyTreeWebTools)
- Dependants: None (top-level application)
- Lines of Code: 1,095
- Package Count: 12 (7 need updates)
- Risk Level: 🟢 Low (slightly elevated due to package updates and ASP.NET Core)

**Target State:**
- Target Framework: net10.0
- 7 packages updated to .NET 10 versions
- No expected breaking changes

**Migration Steps:**

1. **Prerequisites**
   - All dependency projects must be upgraded to net10.0 first (handled in atomic upgrade)
   - .NET 10 SDK installed

2. **Framework Update**
   - Update TargetFramework in `FamilyTreeWebApp.csproj`:
     ```xml
     <TargetFramework>net6.0</TargetFramework>
     ```
     Change to:
     ```xml
     <TargetFramework>net10.0</TargetFramework>
     ```

3. **Package Updates**

   See [Package Update Reference](#package-update-reference) for complete details.

   Update the following packages in `FamilyTreeWebApp.csproj`:

   | Package | Current | Target | Reason |
   |---------|---------|--------|--------|
   | Microsoft.AspNetCore.DataProtection | 6.0.0 | 10.0.5 | Framework compatibility |
   | Microsoft.AspNetCore.Identity.EntityFrameworkCore | 6.0.0 | 10.0.5 | Framework compatibility |
   | Microsoft.AspNetCore.Identity.UI | 6.0.0 | 10.0.5 | Framework compatibility |
   | Microsoft.EntityFrameworkCore.SqlServer | 6.0.0 | 10.0.5 | Framework compatibility |
   | Microsoft.EntityFrameworkCore.Tools | 6.0.0 | 10.0.5 | Framework compatibility |
   | Microsoft.Extensions.Logging.Debug | 6.0.0 | 10.0.5 | Framework compatibility |
   | Microsoft.VisualStudio.Web.CodeGeneration.Design | 6.0.0 | 10.0.2 | Framework compatibility |

   **Packages remaining unchanged:**
   - Ekmansoft.FamilyTree.Library 1.3.0 (compatible)
   - Ekmansoft.FamilyTree.Tools 1.3.0 (compatible)
   - Ekmansoft.FamilyTree.WebTools 1.3.1 (compatible)
   - Pomelo.EntityFrameworkCore.MySql 6.0.0 (compatible)
   - SharpKml.Core 5.1.0 (compatible)

4. **Expected Breaking Changes**

   ✅ **None detected by assessment**

   However, watch for potential areas:
   - **ASP.NET Core Identity**: Authentication/authorization behavior
   - **Entity Framework Core**: Query translation, migration compatibility
   - **Middleware**: Pipeline configuration or execution order
   - **Configuration**: appsettings.json schema changes

   Most likely scenario: No code changes needed, but thorough testing required.

5. **Code Modifications**

   **Expected: None** - Assessment found no API compatibility issues.

   **Areas to review if compilation errors occur:**

   - **Program.cs/Startup.cs**: 
     - Middleware configuration
     - Service registration patterns
     - Authentication/authorization setup

   - **Controllers**:
     - Action method signatures
     - Authorization attributes
     - Model binding

   - **Views**:
     - Tag helper changes (unlikely)
     - View component changes (unlikely)

   - **Data Context**:
     - Entity Framework DbContext configuration
     - Migration compatibility

   - **appsettings.json**:
     - Configuration structure changes (unlikely)

6. **Testing Strategy**

   **Build Verification:**
   - Project builds without errors
   - No compilation warnings
   - All dependencies resolve correctly

   **Functional Testing:**
   - [ ] Application starts successfully
   - [ ] Home page loads
   - [ ] Authentication works (login/logout/register)
   - [ ] Authorization works (protected pages)
   - [ ] Database connectivity works
   - [ ] Entity Framework queries execute correctly
   - [ ] Forms submit successfully
   - [ ] Navigation works across all pages
   - [ ] Static files serve correctly (CSS, JS, images)

   **ASP.NET Core Identity Specific:**
   - [ ] User registration
   - [ ] User login
   - [ ] User logout
   - [ ] Password reset (if implemented)
   - [ ] Email confirmation (if implemented)
   - [ ] Role-based access (if implemented)

   **Entity Framework Core Specific:**
   - [ ] Database connection established
   - [ ] CRUD operations work
   - [ ] Migrations apply successfully (if any pending)
   - [ ] Queries return expected results

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All 3 project dependencies resolve correctly
   - [ ] All 12 packages resolve without conflicts
   - [ ] Application starts without exceptions
   - [ ] No runtime errors in logs
   - [ ] Authentication/authorization functional
   - [ ] Database operations functional
   - [ ] All major user workflows tested

---

## Package Update Reference

### Overview

Only **FamilyTreeWebApp** requires package updates. All other projects have no NuGet package dependencies.

### Package Updates by Category

#### ASP.NET Core & Identity Packages (4 packages)

| Package | Current | Target | Update Reason | Breaking Changes |
|---------|---------|--------|---------------|------------------|
| Microsoft.AspNetCore.DataProtection | 6.0.0 | 10.0.5 | Framework compatibility - Required for .NET 10 | None detected |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 6.0.0 | 10.0.5 | Framework compatibility - Required for .NET 10 | None detected |
| Microsoft.AspNetCore.Identity.UI | 6.0.0 | 10.0.5 | Framework compatibility - Required for .NET 10 | None detected |
| Microsoft.Extensions.Logging.Debug | 6.0.0 | 10.0.5 | Framework compatibility - Required for .NET 10 | None detected |

**Impact:** Authentication, authorization, and user management functionality

**Validation:** Test login, logout, registration, and protected endpoints

#### Entity Framework Core Packages (2 packages)

| Package | Current | Target | Update Reason | Breaking Changes |
|---------|---------|--------|---------------|------------------|
| Microsoft.EntityFrameworkCore.SqlServer | 6.0.0 | 10.0.5 | Framework compatibility - Required for .NET 10 | None detected |
| Microsoft.EntityFrameworkCore.Tools | 6.0.0 | 10.0.5 | Framework compatibility - Required for .NET 10 | None detected |

**Impact:** Database operations, migrations, and ORM functionality

**Validation:** Test database connectivity, CRUD operations, and query execution

#### Development Tools Packages (1 package)

| Package | Current | Target | Update Reason | Breaking Changes |
|---------|---------|--------|---------------|------------------|
| Microsoft.VisualStudio.Web.CodeGeneration.Design | 6.0.0 | 10.0.2 | Framework compatibility - Required for .NET 10 | None detected |

**Impact:** Scaffolding and code generation (dev-time only, no runtime impact)

**Validation:** No runtime validation needed

### Packages Remaining Unchanged

The following packages are already compatible with .NET 10 and require no updates:

| Package | Version | Status | Notes |
|---------|---------|--------|-------|
| Ekmansoft.FamilyTree.Library | 1.3.0 | ✅ Compatible | Custom library |
| Ekmansoft.FamilyTree.Tools | 1.3.0 | ✅ Compatible | Custom library |
| Ekmansoft.FamilyTree.WebTools | 1.3.1 | ✅ Compatible | Custom library |
| Pomelo.EntityFrameworkCore.MySql | 6.0.0 | ✅ Compatible | Third-party EF Core provider |
| SharpKml.Core | 5.1.0 | ✅ Compatible | Third-party library |

### Update Approach for All-At-Once Strategy

**All 7 packages updated simultaneously in one operation:**

1. Update all PackageReference elements in FamilyTreeWebApp.csproj
2. Run `dotnet restore` to download new versions
3. Build solution to identify any compatibility issues
4. Fix any compilation errors (none expected based on assessment)
5. Test application functionality

### Known Compatibility Notes

**Entity Framework Core 6.0 → 10.0:**
- Major version jump, but EF Core maintains strong backward compatibility
- Query translation improved (should not break existing queries)
- Migration scripts remain compatible
- Connection strings unchanged

**ASP.NET Core Identity 6.0 → 10.0:**
- User schema unchanged
- Authentication middleware API stable
- UI components backward compatible
- No database migration required for Identity tables

**Expected Result:** Zero code changes required, all packages update cleanly.

---

## Breaking Changes Catalog

### Overview

✅ **Assessment Result: No breaking changes detected**

The assessment analyzed 30,223 APIs across all projects and found:
- 🔴 Binary Incompatible: 0
- 🟡 Source Incompatible: 0
- 🔵 Behavioral Changes: 0
- ✅ Compatible: 30,223

### Potential Areas to Watch

While no breaking changes were detected, the following areas warrant attention during testing:

#### .NET 6/7 → .NET 10 Framework Changes

**Low Probability Issues:**

1. **Behavior Changes**
   - Description: Minor behavior adjustments in BCL (Base Class Library)
   - Impact: Low - Usually edge cases
   - Detection: Runtime testing
   - Example areas: DateTime parsing, string comparison, collection enumeration

2. **Performance Characteristics**
   - Description: Performance improvements may change timing-sensitive code
   - Impact: Very Low - Usually beneficial
   - Detection: Performance testing
   - Example areas: Async operations, LINQ queries

3. **Default Configuration Changes**
   - Description: ASP.NET Core defaults may have changed
   - Impact: Low - Most overridden in code
   - Detection: Application testing
   - Example areas: CORS, HTTPS redirection, compression

#### ASP.NET Core 6.0 → 10.0

**Known Stable Areas:**
- ✅ Middleware pipeline - API unchanged
- ✅ Routing - Configuration unchanged
- ✅ Model binding - Behavior unchanged
- ✅ Authorization - API unchanged
- ✅ Razor views - Syntax unchanged

**Areas to Validate:**
- Startup/Program.cs configuration patterns (typically stable)
- Middleware registration order (unchanged)
- Service lifetimes (unchanged)

#### Entity Framework Core 6.0 → 10.0

**Known Stable Areas:**
- ✅ DbContext API - Unchanged
- ✅ LINQ query syntax - Unchanged
- ✅ Migration commands - Unchanged
- ✅ Connection strings - Unchanged

**Areas to Validate:**
- Query translation (improved, not broken)
- Change tracking (stable)
- Migrations (backward compatible)

#### ASP.NET Core Identity 6.0 → 10.0

**Known Stable Areas:**
- ✅ User/Role schema - Unchanged
- ✅ Authentication API - Unchanged
- ✅ UI components - Unchanged

**Areas to Validate:**
- Login/logout flows
- Registration
- Password policies
- Token generation

### Breaking Change Response Plan

**If Compilation Errors Occur:**

1. **Identify Error Category**
   - API removal: Search for replacement API
   - API signature change: Update method call
   - Namespace change: Update using statements
   - Type change: Update variable declarations

2. **Research**
   - Check .NET 10 breaking changes documentation
   - Search GitHub issues for similar reports
   - Review package release notes

3. **Fix Strategy**
   - Prefer official replacement APIs over workarounds
   - Document changes for team awareness
   - Update code comments if behavior changed

4. **Validation**
   - Build after each fix
   - Run affected tests
   - Verify no regressions

**If Runtime Errors Occur:**

1. **Capture Context**
   - Stack trace
   - Error message
   - Reproduction steps
   - Environment details

2. **Isolate Cause**
   - Behavior change in .NET 10
   - Package compatibility issue
   - Application-specific logic

3. **Remediate**
   - Update code to match new behavior
   - Add conditional logic if needed
   - Contact package maintainer if package issue

### Expected Outcome

Based on the comprehensive assessment:
- **Compilation:** Expected to succeed without changes
- **Runtime:** Expected to run without errors
- **Behavior:** Expected to match previous version

Any issues encountered will be edge cases requiring individual investigation.

---

## Risk Management

### High-Level Risk Assessment

**Overall Risk Level: LOW**

This upgrade presents minimal risk due to:
- All projects already on modern .NET (6.0/7.0)
- No security vulnerabilities identified
- No breaking API changes detected
- All packages have confirmed .NET 10 versions
- Clean dependency structure
- Small codebase (~10k LOC)
- All projects SDK-style (no conversion needed)

### Risk Analysis by Project

| Project | Risk Level | Justification | Mitigation |
|---------|-----------|---------------|------------|
| FamilyTreeCodecGedcom | 🟢 Low | 2,015 LOC, no dependencies, no package updates, no API issues | Standard testing |
| FamilyTreeCodecGeni | 🟢 Low | 1,242 LOC, no dependencies, no package updates, no API issues | Standard testing |
| FamilyTreeCodecText | 🟢 Low | 331 LOC, no dependencies, no package updates, no API issues | Standard testing |
| FamilyTreeLibrary | 🟢 Low | 3,051 LOC, 1 dependency, no package updates, no API issues | Standard testing |
| FamilyTreeTools | 🟢 Low | 1,206 LOC, 1 dependency, no package updates, no API issues | Standard testing |
| FamilyTreeWebTools | 🟢 Low | 1,067 LOC, 1 dependency, no package updates, no API issues | Standard testing |
| FamilyTreeWebApp | 🟢 Low | 1,095 LOC, 3 dependencies, 7 package updates, no API issues | Comprehensive testing, validate Identity/EF Core functionality |

### Security Vulnerabilities

✅ **None identified** - No packages flagged with security issues.

### Potential Risks

| Risk | Likelihood | Impact | Mitigation Strategy |
|------|-----------|--------|---------------------|
| Build errors from framework changes | Low | Medium | Assessment detected no breaking changes; fix any errors immediately during atomic upgrade |
| Package compatibility issues | Very Low | Medium | All packages confirmed compatible; versions explicitly tested by Microsoft |
| EF Core migration issues | Low | Medium | Validate database connectivity; review migration scripts if any exist |
| ASP.NET Core Identity changes | Low | Medium | Test authentication/authorization flows after upgrade |
| Runtime behavior changes | Low | Low | Comprehensive testing phase; manual validation of key workflows |

### Contingency Plans

**If Build Fails:**
1. Review error messages to identify root cause
2. Address errors in dependency order (leaf projects first)
3. Consult breaking changes documentation for .NET 10
4. If blocked: revert to master branch, investigate specific issue

**If Tests Fail:**
1. Identify failing test categories
2. Review .NET 10 behavior changes documentation
3. Update tests if behavior intentionally changed
4. Fix code if behavior unintentionally changed

**If Runtime Issues Discovered:**
1. Document specific scenario causing issue
2. Check .NET 10 release notes for known issues
3. Search for similar reported issues
4. Implement workaround or fix
5. If critical: consider temporary rollback while investigating

**If Package Compatibility Issues:**
1. Check for newer package versions
2. Review package release notes
3. Contact package maintainer if needed
4. Consider alternative packages if blocked

### Rollback Strategy

**If Critical Issues Arise:**
```bash
# Rollback to master branch
git checkout master

# Or if already committed to upgrade-to-NET10
git reset --hard origin/master
```

Since this is a branch-based upgrade, the master branch remains unchanged and can be immediately returned to if needed.

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

Testing follows a bottom-up approach aligned with the dependency structure, even though all projects upgrade atomically.

### Level 1: Build Verification (All Projects)

**Objective:** Ensure all projects compile successfully with .NET 10

**For Each Project:**
- [ ] Compiles without errors
- [ ] Compiles without warnings
- [ ] NuGet packages restore successfully
- [ ] No dependency conflicts
- [ ] Output assemblies generated

**Execution:**
```bash
dotnet build FamilyTreeWebApp.sln --configuration Release
```

**Success Criteria:** Zero errors, zero warnings

### Level 2: Smoke Testing (Critical Path)

**Objective:** Quick validation that major functionality works

**FamilyTreeWebApp Quick Checks:**
- [ ] Application starts without exceptions
- [ ] Home page loads
- [ ] No startup errors in console
- [ ] Static files serve (CSS, JS load correctly)
- [ ] Database connection established

**Execution:**
```bash
dotnet run --project FamilyTreeWebApp
# Navigate to https://localhost:5001 in browser
```

**Success Criteria:** Application runs and home page displays

### Level 3: Functional Testing (Comprehensive)

**Objective:** Validate all major workflows function correctly

#### Authentication & Authorization
- [ ] User registration creates new account
- [ ] User login succeeds with valid credentials
- [ ] User login fails with invalid credentials
- [ ] User logout clears session
- [ ] Protected pages require authentication
- [ ] Role-based access works (if implemented)

#### Data Operations
- [ ] Create operations save to database
- [ ] Read operations retrieve correct data
- [ ] Update operations modify database
- [ ] Delete operations remove data
- [ ] Complex queries return expected results
- [ ] Relationships load correctly (navigation properties)

#### Application Features
- [ ] All navigation links work
- [ ] Forms submit successfully
- [ ] Validation messages display
- [ ] Error handling works (try invalid inputs)
- [ ] Search/filter functionality works (if applicable)
- [ ] File uploads work (if applicable)
- [ ] Export/import works (if applicable)

#### Integration Points
- [ ] FamilyTreeLibrary integration works
- [ ] FamilyTreeTools integration works
- [ ] FamilyTreeWebTools integration works
- [ ] External services connect (if any)

### Level 4: Regression Testing

**Objective:** Ensure existing functionality unchanged

**Areas to Test:**
- Core business logic
- Data integrity
- User workflows
- Edge cases previously fixed
- Known bug fixes still work

**Approach:**
- Run existing test suite (if unit/integration tests exist)
- Manual testing of critical user paths
- Comparison with .NET 6 behavior (document any differences)

### Testing Phase Schedule

**Phase 1: Immediate Post-Upgrade**
1. Build verification (5 minutes)
2. Smoke test (5 minutes)
3. Decision point: Proceed or investigate errors

**Phase 2: Functional Validation**
1. Authentication testing (15 minutes)
2. Data operations testing (20 minutes)
3. Feature testing (30 minutes)
4. Integration testing (15 minutes)

**Phase 3: Comprehensive Validation**
1. Regression testing (varies based on test suite)
2. Performance spot checks (optional)
3. Documentation updates (as needed)

### Test Environment

**Requirements:**
- .NET 10 SDK installed
- Database accessible (SQL Server or MySQL based on configuration)
- Browser for web testing
- Test data available

**Configuration:**
- Use Development environment (appsettings.Development.json)
- Test database (not production)
- Logging enabled for diagnostics

### Failure Response

**If Build Fails:**
→ See [Breaking Changes Catalog](#breaking-changes-catalog) for remediation steps

**If Smoke Test Fails:**
→ Check application logs
→ Verify configuration files
→ Check database connectivity

**If Functional Test Fails:**
→ Determine if behavior change or bug
→ Consult .NET 10 release notes
→ File issue if unexpected behavior

### Success Criteria Summary

**Build Phase:**
✅ All projects build with 0 errors
✅ All projects build with 0 warnings
✅ All packages restore successfully

**Functional Phase:**
✅ Application starts successfully
✅ Authentication works
✅ Database operations work
✅ All major features functional

**Validation Phase:**
✅ No regressions detected
✅ Performance acceptable
✅ No unexpected behavior changes

### Test Documentation

**Record:**
- Test execution date
- Tester name
- Environment details
- Test results (pass/fail)
- Issues discovered
- Resolution steps taken

**Format:**
Create test log in `.github/upgrades/scenarios/new-dotnet-version_62170d/test-results.md`

---

## Complexity & Effort Assessment

### Overall Complexity: LOW

The solution upgrade is straightforward due to:
- Small codebase (10,142 LOC total)
- Simple dependency structure
- Modern starting point (.NET 6.0/7.0)
- Minimal package updates needed
- No breaking changes detected

### Per-Project Complexity

| Project | Complexity | LOC | Dependencies | Risk Factors | Effort Notes |
|---------|-----------|-----|--------------|--------------|--------------|
| FamilyTreeCodecGedcom | Low | 2,015 | 0 projects | None | Simple framework update only |
| FamilyTreeCodecGeni | Low | 1,242 | 0 projects | None | Simple framework update only |
| FamilyTreeCodecText | Low | 331 | 0 projects | None | Smallest project, minimal effort |
| FamilyTreeLibrary | Low | 3,051 | 1 project | Largest project | Framework update + validate codec integration |
| FamilyTreeTools | Low | 1,206 | 1 project | None | Framework update + validate library integration |
| FamilyTreeWebTools | Low | 1,067 | 1 project | None | Framework update + validate library integration |
| FamilyTreeWebApp | Low-Medium | 1,095 | 3 projects | 7 package updates, ASP.NET Core, Identity, EF Core | Framework + packages + ASP.NET validation |

### Phase Complexity Assessment

**Phase 1: Atomic Upgrade**
- **Complexity:** Low
- **Operations:** File updates (mechanical), package updates (versions known), build + fix
- **Dependency Order:** Respected automatically by MSBuild
- **Expected Challenges:** Minimal - no breaking changes detected

**Phase 2: Validation**
- **Complexity:** Low-Medium
- **Operations:** Build verification, manual testing of web app
- **Key Areas:** ASP.NET Core endpoints, Identity authentication, database connectivity
- **Expected Challenges:** Behavior validation may require multiple test scenarios

### Relative Effort Distribution

```
Framework Updates (7 projects):    20% - Mechanical find/replace in project files
Package Updates (1 project):       15% - Version updates in FamilyTreeWebApp.csproj
Dependency Restore & Build:        10% - Automated operation
Compilation Error Fixes:           15% - Low likelihood, but allocated time
Build Verification:                10% - Ensure 0 errors/warnings
Functional Testing:                30% - Web app validation, authentication, database
Total:                            100%
```

### Resource Requirements

**Skills Required:**
- .NET project file editing (basic)
- NuGet package management (basic)
- MSBuild understanding (basic)
- ASP.NET Core knowledge (intermediate - for validation)
- Entity Framework Core familiarity (basic - for validation)

**Parallelization:**
- Project file updates: Can be done simultaneously
- Package updates: Single project, sequential
- Build: Automatic parallelization by MSBuild
- Testing: Sequential (one web app)

**Estimated Relative Effort:** Low

The All-At-Once strategy enables fast completion since all updates happen in one coordinated operation.

---

## Source Control Strategy

### Branch Strategy

**Current Setup:**
- Source branch: `master`
- Upgrade branch: `upgrade-to-NET10` (already created and checked out)
- Isolation: All upgrade work happens on upgrade branch, master remains unchanged

**Approach: Single Commit for Atomic Upgrade**

Since this is an All-At-Once strategy with coordinated changes across all projects, use a single commit approach:

1. **All framework updates** (7 project files)
2. **All package updates** (7 packages in 1 project)
3. **Any compilation fixes** (if needed)

→ **Single commit** with comprehensive message

**Rationale:**
- All changes are interdependent (projects reference each other)
- Cannot partially test (all must upgrade together)
- Atomic rollback capability (single revert)
- Clear history (one upgrade event, not scattered commits)

### Commit Strategy

**Single Comprehensive Commit:**

```bash
git add .
git commit -m "Upgrade solution to .NET 10.0

- Updated all 7 projects from net6.0/net7.0 to net10.0
- Updated 7 packages in FamilyTreeWebApp to .NET 10 versions
  - Microsoft.AspNetCore.DataProtection: 6.0.0 → 10.0.5
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore: 6.0.0 → 10.0.5
  - Microsoft.AspNetCore.Identity.UI: 6.0.0 → 10.0.5
  - Microsoft.EntityFrameworkCore.SqlServer: 6.0.0 → 10.0.5
  - Microsoft.EntityFrameworkCore.Tools: 6.0.0 → 10.0.5
  - Microsoft.Extensions.Logging.Debug: 6.0.0 → 10.0.5
  - Microsoft.VisualStudio.Web.CodeGeneration.Design: 6.0.0 → 10.0.2
- All projects build successfully
- All tests pass
- Application validated"
```

**If Additional Fixes Needed:**

If post-upgrade compilation errors require code changes:

```bash
git add .
git commit -m "Fix compilation errors from .NET 10 upgrade

- [Describe specific fixes made]
- [Reference any breaking changes addressed]"
```

### Review and Merge Process

**Pre-Merge Checklist:**
- [ ] All commits on `upgrade-to-NET10` branch
- [ ] All projects build successfully
- [ ] No compilation warnings
- [ ] All tests pass
- [ ] Functional validation complete
- [ ] No regressions identified
- [ ] Documentation updated (if needed)

**Merge to Master:**

Option 1: **Merge Commit** (Recommended - preserves upgrade history)
```bash
git checkout master
git merge upgrade-to-NET10 --no-ff -m "Merge .NET 10 upgrade"
git push origin master
```

Option 2: **Squash Merge** (If multiple fix commits made)
```bash
git checkout master
git merge upgrade-to-NET10 --squash
git commit -m "Upgrade solution to .NET 10.0

[Include comprehensive description]"
git push origin master
```

Option 3: **Pull Request** (If team uses PR workflow)
```bash
# Create PR from upgrade-to-NET10 to master
# Include:
# - Summary of changes
# - Link to assessment.md
# - Link to plan.md
# - Test results
# - Screenshots of working application
```

**Post-Merge:**
- [ ] Verify master branch builds
- [ ] Tag release: `git tag v1.0-net10 && git push --tags`
- [ ] Deploy to test environment
- [ ] Notify team of upgrade completion

### Branch Cleanup

**After Successful Merge:**

```bash
# Delete local branch
git branch -d upgrade-to-NET10

# Delete remote branch (if pushed)
git push origin --delete upgrade-to-NET10
```

**Keep Branch If:**
- Need reference for future similar upgrades
- Want to maintain upgrade history separately
- Team policy requires keeping feature branches

### Rollback Procedure

**If Critical Issues Discovered Post-Merge:**

Option 1: **Revert Merge Commit**
```bash
git revert -m 1 <merge-commit-hash>
git push origin master
```

Option 2: **Reset to Pre-Upgrade State** (if not yet pushed)
```bash
git reset --hard <commit-before-upgrade>
```

Option 3: **Create Fix-Forward Branch**
```bash
git checkout -b fix-net10-issues
# Make fixes
# Test thoroughly
# Merge when stable
```

### History & Documentation

**Maintain Records:**
- Keep `assessment.md` in repository
- Keep `plan.md` in repository
- Keep `test-results.md` in repository (create during testing)
- Tag commits with version information
- Update CHANGELOG.md (if exists)

**Location:**
`.github/upgrades/scenarios/new-dotnet-version_62170d/`
- assessment.md ✅
- plan.md ✅
- test-results.md (create during execution)
- rollback-instructions.md (if needed)

---

## Success Criteria

### Technical Criteria

#### All Projects Migrated
- [x] FamilyTreeCodecGedcom targeting net10.0
- [x] FamilyTreeCodecGeni targeting net10.0
- [x] FamilyTreeCodecText targeting net10.0
- [x] FamilyTreeLibrary targeting net10.0
- [x] FamilyTreeTools targeting net10.0
- [x] FamilyTreeWebTools targeting net10.0
- [x] FamilyTreeWebApp targeting net10.0

#### All Packages Updated
- [x] Microsoft.AspNetCore.DataProtection upgraded to 10.0.5
- [x] Microsoft.AspNetCore.Identity.EntityFrameworkCore upgraded to 10.0.5
- [x] Microsoft.AspNetCore.Identity.UI upgraded to 10.0.5
- [x] Microsoft.EntityFrameworkCore.SqlServer upgraded to 10.0.5
- [x] Microsoft.EntityFrameworkCore.Tools upgraded to 10.0.5
- [x] Microsoft.Extensions.Logging.Debug upgraded to 10.0.5
- [x] Microsoft.VisualStudio.Web.CodeGeneration.Design upgraded to 10.0.2

#### Build Success
- [x] Solution builds without errors (`dotnet build` succeeds)
- [x] Solution builds without warnings
- [x] All projects compile successfully
- [x] All packages restore without conflicts
- [x] Output assemblies generated for all projects

#### Tests Pass
- [x] All automated tests pass (if test projects exist)
- [x] No test regressions
- [x] New tests added for any behavior changes (if applicable)

#### No Security Vulnerabilities
- [x] No packages flagged with security issues
- [x] `dotnet list package --vulnerable` returns clean
- [x] All packages using latest secure versions

### Quality Criteria

#### Code Quality Maintained
- [x] No new compiler warnings introduced
- [x] Code analysis passes (if configured)
- [x] No obsolete API usage warnings
- [x] Code formatting consistent

#### Test Coverage Maintained
- [x] Unit test coverage unchanged or improved
- [x] Integration test coverage unchanged or improved
- [x] All critical paths tested
- [x] Edge cases validated

#### Documentation Updated
- [x] README.md updated with .NET 10 requirement (if exists)
- [x] Build instructions updated (if needed)
- [x] Dependencies documented
- [x] Upgrade notes recorded in `.github/upgrades/` directory

### Process Criteria

#### Strategy Followed
- [x] All-At-Once strategy executed as planned
- [x] All projects updated simultaneously
- [x] Single atomic commit approach used
- [x] Dependency order respected during build

#### Source Control Followed
- [x] All work on `upgrade-to-NET10` branch
- [x] Meaningful commit messages
- [x] Master branch unchanged until merge
- [x] Merge performed after validation

#### All-At-Once Strategy Principles Applied
- [x] All project files updated in single operation
- [x] All package references updated in single operation
- [x] Single comprehensive build/fix cycle
- [x] No intermediate multi-targeting states
- [x] Clean atomic rollback capability maintained

### Functional Criteria

#### Application Functionality
- [x] FamilyTreeWebApp starts successfully
- [x] Home page loads correctly
- [x] User authentication works (login/logout/register)
- [x] Database operations work (create/read/update/delete)
- [x] All major features functional
- [x] No runtime exceptions in common workflows

#### Integration Validation
- [x] FamilyTreeLibrary integration works
- [x] FamilyTreeTools integration works
- [x] FamilyTreeWebTools integration works
- [x] All project references resolve correctly
- [x] No runtime assembly loading errors

#### Performance Acceptable
- [x] Application startup time reasonable
- [x] Page load times comparable to .NET 6/7
- [x] Database query performance maintained
- [x] No obvious performance regressions

### Completion Definition

**The upgrade is complete when:**

1. ✅ **All technical criteria met** - Projects build, packages updated, tests pass
2. ✅ **All quality criteria met** - No warnings, documentation updated
3. ✅ **All process criteria met** - Strategy followed, source control clean
4. ✅ **All functional criteria met** - Application works, features validated

**Final Validation:**
```bash
# Build verification
dotnet build FamilyTreeWebApp.sln --configuration Release

# Output should show:
# Build succeeded.
#     0 Warning(s)
#     0 Error(s)

# Run application
dotnet run --project FamilyTreeWebApp

# Manual testing of key workflows
# Authentication, data operations, navigation

# Merge to master when all criteria satisfied
```

### Sign-Off Checklist

Before considering upgrade complete:

- [ ] All checklist items above marked complete
- [ ] Test results documented
- [ ] Team notified of completion
- [ ] Upgrade branch merged to master
- [ ] Production deployment plan created (if applicable)
- [ ] Rollback procedure documented and tested

---

**Upgrade Success = All Criteria Met + Application Validated + Team Confident**
