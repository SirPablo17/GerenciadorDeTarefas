## Purpose

Define o comportamento observável do cliente web: autenticar-se, visualizar as próprias tarefas e encerrar a sessão, incluindo o que o usuário vê em cada estado (carregando, erro, sessão expirada).

## ADDED Requirements

### Requirement: Unauthenticated Access Redirects to Login
The system SHALL redirect the user to the login screen when they attempt to access any protected screen without an active session.

#### Scenario: Protected screen visited without a session
- **WHEN** a user without an active session navigates directly to a protected screen (e.g. the task list)
- **THEN** the system shows the login screen instead of the protected screen

### Requirement: Successful Login Navigates to the Task List
The system SHALL start an authenticated session and show the task list when the user submits valid credentials.

#### Scenario: Valid credentials submitted
- **WHEN** a user submits a registered email and its matching password on the login screen
- **THEN** the system starts an authenticated session and shows the user's task list

### Requirement: Invalid Login Credentials Are Rejected Without Losing the User's Place
The system SHALL keep the user on the login screen and communicate the failure clearly when submitted credentials are rejected, without retaining the previously typed password.

#### Scenario: Incorrect email or password submitted
- **WHEN** a user submits an email/password combination the system does not accept as valid
- **THEN** the system keeps the user on the login screen, displays a clear message that the credentials are invalid, and clears the password field while leaving the email field as typed

### Requirement: Task List Reflects Loading, Error, and Empty States
The system SHALL show the authenticated user's own tasks and SHALL distinguish, in what the user sees, between the list loading, the list failing to load, and the list being empty.

#### Scenario: Task list is loading
- **WHEN** the authenticated user opens the task list and the tasks have not yet been retrieved
- **THEN** the system shows a loading indication and does not show stale or placeholder task data

#### Scenario: Task list fails to load
- **WHEN** the authenticated user's tasks cannot be retrieved
- **THEN** the system shows an error message and offers a way to retry the retrieval, and retrying re-attempts loading the tasks

#### Scenario: Authenticated user has no tasks
- **WHEN** the authenticated user's task retrieval succeeds and returns no tasks
- **THEN** the system shows an empty-state message instead of a blank list

#### Scenario: Authenticated user has tasks
- **WHEN** the authenticated user's task retrieval succeeds and returns one or more tasks
- **THEN** the system shows those tasks, and only tasks belonging to the authenticated user

### Requirement: Logout Ends the Session and Clears Application State
The system SHALL end the session, clear all in-application state derived from that session, and return the user to the login screen when they log out, such that navigating back afterward does not reveal data from the ended session.

#### Scenario: User logs out
- **WHEN** an authenticated user chooses to log out
- **THEN** the system ends the session, clears any task data and user information held from that session, and shows the login screen

#### Scenario: Browser back navigation after logout
- **WHEN** a user who just logged out uses the browser's back navigation
- **THEN** the system does not display task data or any other content from the ended session

### Requirement: Expired Session Redirects to Login and Restores the Original Destination
The system SHALL detect that the current session is no longer valid, end it, and send the user to the login screen; after the user re-authenticates, the system SHALL return them to the screen they had been trying to reach.

#### Scenario: Session expires while viewing the task list
- **WHEN** the user's session is no longer accepted by the system while they are using a protected screen
- **THEN** the system ends the session and shows the login screen

#### Scenario: Re-authenticating after an expired session
- **WHEN** a user logs in again after being redirected to login due to an expired session
- **THEN** the system takes them to the protected screen they had originally been trying to reach, not to a default screen
