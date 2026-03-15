# Project Overview

This project is a backend for a patient rehabilitation system used by clinics.

Technology stack:
- C#
- ASP.NET Core Web API
- EF Core
- PostgreSQL

Solution structure:
- RehabApp.Domain
- RehabApp.Application
- RehabApp.Infrastructure
- RehabApp.Api

The project should be structured and maintainable, but it does not need to strictly follow Clean Architecture rules.
Practicality and clarity are more important than architectural purity.

# Core Business Context

The system supports rehabilitation workflows for clinics.

Main roles:
- Admin
- Doctor
- Patient

Each user belongs to exactly one clinic.

Clinic data must be isolated.

Admins manage users.
Doctors create and assign rehabilitation content.
Patients follow assigned rehabilitation plans and report progress.

# Multi-Tenancy Rules

The system is multi-tenant by clinic.

Rules:
- every user belongs to a clinic
- users must not access data from another clinic
- admins manage only users in their own clinic
- doctors and patients must not cross clinic boundaries

All read and write operations must enforce clinic isolation.

# Global vs Clinic Content

Some entities may later be global/system-level.

Global entities:
- have `ClinicId = null`
- have `CreatedByUserId = null`

Future examples:
- exercises
- workouts
- training plans

Global content is shared across clinics.

Clinic-specific content:
- has `ClinicId`
- may have `CreatedByUserId`

# Main Domain Concepts

Important domain entities for the project as a whole:
- Clinic
- User
- Exercise
- Workout
- WorkoutItem
- TrainingPlan
- TrainingPlanDay
- PatientTrainingPlan
- PatientTrainingPlanDay
- PatientWorkoutItemProgress

Conceptual relations:
- users belong to clinics
- doctors and patients may have many-to-many relation
- doctors create exercises, workouts, and training plans
- training plans contain dated workout days
- doctors assign plans to patients
- patients report progress

Progress tracking should eventually support:
- overall workout progress
- total completed exercises
- overall comment for the whole training
- per-exercise progress
- per-exercise comments

# Current User Rules

Users:
- are created by admins
- authenticate by email and password
- store password hashes only
- have role-based access
- can be deactivated via `IsActive = false`

Inactive users must not be able to authenticate.

There is no public registration.

# Project Structure Guidelines

The project is organized into 4 projects for clarity:

## RehabApp.Domain
Contains domain entities, enums, and core business models.

## RehabApp.Application
Contains application services, use cases, DTOs, validation, and orchestration logic.

## RehabApp.Infrastructure
Contains EF Core persistence, database configuration, migrations, authentication services, password hashing, JWT generation, and other technical integrations.

## RehabApp.Api
Contains controllers, HTTP contracts, dependency injection setup, authentication setup, authorization setup, Swagger, middleware, and ProblemDetails configuration.

This structure is intended for organization, not strict architectural isolation.

# API Design Rules

General API conventions:
- REST API
- JSON
- no versioning for now
- use `ProblemDetails` for error responses
- use request/response DTOs
- do not expose persistence entities directly from controllers
- add pagination where relevant
- validate incoming request models
- use GUID ids

# Authentication and Authorization

Authentication:
- JWT bearer authentication
- email + password login
- no refresh tokens for now

Authorization:
- role-based authorization is enough for MVP
- roles are:
    - Admin
    - Doctor
    - Patient

# Persistence Rules

Database:
- PostgreSQL
- EF Core

Rules:
- use migrations
- keep entity mappings explicit
- keep relationships explicit
- avoid accidental cascade delete behavior

# Development Principles

General principles:
- keep code simple and explicit
- prefer readability over overengineering
- avoid unnecessary abstractions
- keep responsibilities reasonably separated
- controllers should stay thin
- business logic should not be scattered randomly across the project

# Seed Data

For development, seed minimal initial data:
- at least one clinic
- at least one admin user
- optionally one doctor and one patient

Passwords in seed data must be hashed, never stored in plain text.