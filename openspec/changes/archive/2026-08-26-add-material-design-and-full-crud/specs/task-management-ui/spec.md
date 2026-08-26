## ADDED Requirements

### Requirement: User Registration Creates a New Account
The system SHALL allow a visitor to create a new account with an email and a password, and SHALL communicate clearly why an attempt was rejected instead of creating an account.

#### Scenario: Successful registration
- **WHEN** a visitor submits a unique, valid email and a password meeting the app's minimum requirements on the registration screen
- **THEN** the system creates the account and gives the visitor access to their (empty) task list without requiring them to type their credentials a second time

#### Scenario: Duplicate email rejected
- **WHEN** a visitor submits an email that already belongs to an existing account
- **THEN** the system keeps the visitor on the registration screen, displays a clear message that the email is already in use, and does not create a new account

#### Scenario: Password not meeting requirements rejected
- **WHEN** a visitor submits a password that does not meet the app's minimum requirements, or an invalid email
- **THEN** the system keeps the visitor on the registration screen, displays a clear message describing what is invalid, and does not create an account

### Requirement: Task Creation
The system SHALL allow the authenticated user to create a new task with a title, an optional description, and a status, and SHALL reject invalid input without creating a task.

#### Scenario: Successful task creation
- **WHEN** the authenticated user submits a new task with a non-empty title
- **THEN** the system creates the task and shows it in the task list

#### Scenario: Invalid task input rejected
- **WHEN** the authenticated user submits a new task with a missing title, or with a title or description longer than the system accepts
- **THEN** the system keeps the user on the task form, displays a clear message describing what is invalid, and does not create a task

### Requirement: Task Editing
The system SHALL allow the authenticated user to edit the title, description, and status of one of their own existing tasks, and SHALL reject invalid input without saving.

#### Scenario: Successful task edit
- **WHEN** the authenticated user submits changes to one of their own tasks with a non-empty title
- **THEN** the system saves the changes and reflects them in the task list

#### Scenario: Invalid task edit rejected
- **WHEN** the authenticated user submits changes to one of their own tasks with a missing title, or with a title or description longer than the system accepts
- **THEN** the system keeps the user on the task form, displays a clear message describing what is invalid, and does not save the changes

### Requirement: Task Status Can Be Changed Without the Full Edit Form
The system SHALL let the authenticated user change a task's status, including marking it complete, directly from the task list, without opening the full edit form.

#### Scenario: Changing status from the task list
- **WHEN** the authenticated user changes the status of one of their own tasks from the task list
- **THEN** the system saves the new status and reflects it in the task list

### Requirement: Task Deletion Requires Confirmation
The system SHALL let the authenticated user permanently delete one of their own tasks, and SHALL require the user to confirm the action before it takes effect.

#### Scenario: Confirmed deletion
- **WHEN** the authenticated user chooses to delete one of their own tasks and confirms the action
- **THEN** the system deletes the task and removes it from the task list

#### Scenario: Cancelled deletion
- **WHEN** the authenticated user starts deleting one of their own tasks but does not confirm the action
- **THEN** the system does not delete the task, and it remains in the task list
