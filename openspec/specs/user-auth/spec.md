# user-auth Specification

## Purpose

Permite que usuários se registrem e façam login na API, recebendo um token JWT usado para autenticar e autorizar o acesso aos demais recursos do sistema.

## Requirements

### Requirement: User Registration
The system SHALL allow a new user to register with an email and a password, rejecting duplicate emails and invalid input.

#### Scenario: Successful registration
- **WHEN** a client submits a registration request with a unique, valid email and a password meeting the minimum requirements
- **THEN** the system creates a new user account with the password stored as a secure hash (never in plain text) and returns a success response

#### Scenario: Duplicate email rejected
- **WHEN** a client submits a registration request with an email that already belongs to an existing user
- **THEN** the system rejects the request with a client error response and does not create a new account

#### Scenario: Invalid registration input rejected
- **WHEN** a client submits a registration request with a missing/malformed email or a password that does not meet the minimum requirements
- **THEN** the system rejects the request with a validation error response describing the invalid fields, and does not create an account

### Requirement: User Login
The system SHALL authenticate a user by email and password and, on success, issue a JWT access token.

#### Scenario: Successful login
- **WHEN** a client submits valid credentials (email and password) for an existing, registered user
- **THEN** the system returns a success response containing a JWT access token identifying that user

#### Scenario: Invalid credentials rejected
- **WHEN** a client submits a login request with an unknown email or an incorrect password
- **THEN** the system rejects the request with an authentication error response and does not issue a token

### Requirement: Protected Resource Access
The system SHALL require a valid JWT access token to access task management endpoints, and SHALL reject requests without one.

#### Scenario: Request without token rejected
- **WHEN** a client calls a task management endpoint without an `Authorization` bearer token
- **THEN** the system rejects the request with an unauthorized error response

#### Scenario: Request with invalid or expired token rejected
- **WHEN** a client calls a task management endpoint with a malformed, invalid, or expired JWT token
- **THEN** the system rejects the request with an unauthorized error response

#### Scenario: Request with valid token allowed
- **WHEN** a client calls a task management endpoint with a valid, non-expired JWT token issued by the system
- **THEN** the system identifies the requesting user from the token and processes the request
