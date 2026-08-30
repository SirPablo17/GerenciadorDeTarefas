## MODIFIED Requirements

### Requirement: Create Task
The system SHALL allow an authenticated user to create a new task owned by that user, requiring at minimum a non-empty title. The system SHALL assign the task a sequential number, unique among that user's tasks, starting at 1 for the user's first task.

#### Scenario: Successful task creation
- **WHEN** an authenticated user submits a new task with a valid, non-empty title
- **THEN** the system creates the task associated with that user and returns the created task, including a generated identifier and its assigned task number

#### Scenario: Task creation rejected with invalid input
- **WHEN** an authenticated user submits a new task with a missing or empty title
- **THEN** the system rejects the request with a validation error response and does not create a task

#### Scenario: Task numbers increment sequentially for a user
- **WHEN** an authenticated user creates multiple tasks over time
- **THEN** each new task receives a number one greater than the previous task created by that same user, starting at 1 for their first task

#### Scenario: Task numbers are isolated per user
- **WHEN** two different authenticated users each create their first task
- **THEN** both tasks are assigned number 1, independently of each other

#### Scenario: Task numbers are not reused after deletion
- **WHEN** an authenticated user deletes a task and then creates a new task
- **THEN** the new task receives a number greater than any number previously assigned to that user, never reusing a deleted task's number

### Requirement: List Own Tasks
The system SHALL allow an authenticated user to list only the tasks that belong to them, optionally filtered by status.

#### Scenario: List returns only the caller's tasks
- **WHEN** an authenticated user requests the list of tasks
- **THEN** the system returns all tasks owned by that user and none belonging to other users

#### Scenario: List without a filter is unaffected
- **WHEN** an authenticated user requests the list of tasks without specifying a status filter
- **THEN** the system returns all of that user's tasks regardless of status, as before

#### Scenario: List filtered by status
- **WHEN** an authenticated user requests the list of tasks with a status filter (e.g., Completed)
- **THEN** the system returns only that user's tasks matching the given status, and none with a different status

#### Scenario: List rejected with an invalid status filter
- **WHEN** an authenticated user requests the list of tasks with a status filter value that is not a recognized task status
- **THEN** the system rejects the request with a validation error response
