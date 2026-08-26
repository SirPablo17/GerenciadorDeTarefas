# task-management Specification

## Purpose

Permite que um usuário autenticado crie, consulte, atualize e remova suas próprias tarefas, com cada tarefa isolada por usuário (multi-tenant).

## Requirements

### Requirement: Create Task
The system SHALL allow an authenticated user to create a new task owned by that user, requiring at minimum a non-empty title.

#### Scenario: Successful task creation
- **WHEN** an authenticated user submits a new task with a valid, non-empty title
- **THEN** the system creates the task associated with that user and returns the created task, including a generated identifier

#### Scenario: Task creation rejected with invalid input
- **WHEN** an authenticated user submits a new task with a missing or empty title
- **THEN** the system rejects the request with a validation error response and does not create a task

### Requirement: List Own Tasks
The system SHALL allow an authenticated user to list only the tasks that belong to them.

#### Scenario: List returns only the caller's tasks
- **WHEN** an authenticated user requests the list of tasks
- **THEN** the system returns all tasks owned by that user and none belonging to other users

### Requirement: Get Task by Id
The system SHALL allow an authenticated user to retrieve a single task they own by its identifier.

#### Scenario: Successful retrieval
- **WHEN** an authenticated user requests a task by id that exists and is owned by that user
- **THEN** the system returns that task's details

#### Scenario: Task not found
- **WHEN** an authenticated user requests a task by id that does not exist
- **THEN** the system returns a not-found error response

#### Scenario: Task owned by another user is not accessible
- **WHEN** an authenticated user requests a task by id that exists but is owned by a different user
- **THEN** the system returns a not-found error response, without revealing that the task belongs to someone else

### Requirement: Update Task
The system SHALL allow an authenticated user to update the fields of a task they own.

#### Scenario: Successful update
- **WHEN** an authenticated user submits valid updated fields for a task they own
- **THEN** the system updates the task and returns the updated task's details

#### Scenario: Update rejected with invalid input
- **WHEN** an authenticated user submits an update with a missing or empty title
- **THEN** the system rejects the request with a validation error response and does not modify the task

#### Scenario: Update of another user's task rejected
- **WHEN** an authenticated user attempts to update a task owned by a different user
- **THEN** the system returns a not-found error response and does not modify the task

### Requirement: Delete Task
The system SHALL allow an authenticated user to delete a task they own.

#### Scenario: Successful deletion
- **WHEN** an authenticated user requests deletion of a task they own
- **THEN** the system deletes the task and subsequent retrieval of that id returns a not-found error response

#### Scenario: Deletion of another user's task rejected
- **WHEN** an authenticated user attempts to delete a task owned by a different user
- **THEN** the system returns a not-found error response and does not delete the task

#### Scenario: Deletion of non-existent task rejected
- **WHEN** an authenticated user requests deletion of a task id that does not exist
- **THEN** the system returns a not-found error response
