## MODIFIED Requirements

### Requirement: Task List Reflects Loading, Error, and Empty States
The system SHALL show the authenticated user's own tasks, split into an Active tab (Pending and In Progress tasks) and a Completed tab, and SHALL distinguish, in what the user sees, between the list loading, the list failing to load, and each tab being empty.

#### Scenario: Task list is loading
- **WHEN** the authenticated user opens the task list and the tasks have not yet been retrieved
- **THEN** the system shows a loading indication and does not show stale or placeholder task data

#### Scenario: Task list fails to load
- **WHEN** the authenticated user's tasks cannot be retrieved
- **THEN** the system shows an error message and offers a way to retry the retrieval, and retrying re-attempts loading the tasks

#### Scenario: Authenticated user has no tasks
- **WHEN** the authenticated user's task retrieval succeeds and returns no tasks
- **THEN** the system shows an empty-state message on the Active tab instead of a blank list

#### Scenario: Authenticated user has tasks
- **WHEN** the authenticated user's task retrieval succeeds and returns one or more tasks
- **THEN** the system shows those tasks in whichever tab matches each task's status, and only tasks belonging to the authenticated user

#### Scenario: Active tab is empty while completed tasks exist
- **WHEN** the authenticated user has one or more completed tasks but no Pending or In Progress tasks
- **THEN** the Active tab shows an empty-state message instead of a blank list

#### Scenario: Completed tab has no completed tasks
- **WHEN** the authenticated user views the Completed tab and has no tasks with status Completed, whether or not they have any Active (Pending or In Progress) tasks
- **THEN** the Completed tab shows an empty-state message instead of a blank list

## ADDED Requirements

### Requirement: Tasks Are Organized Into Active and Completed Tabs
The system SHALL present the authenticated user's tasks in two tabs — Active (Pending and In Progress) and Completed — defaulting to the Active tab, and SHALL move a task between tabs immediately when its status crosses the Completed boundary.

#### Scenario: Default tab on opening the task list
- **WHEN** the authenticated user opens the task list
- **THEN** the system shows the Active tab, containing only Pending and In Progress tasks

#### Scenario: Switching to the Completed tab
- **WHEN** the authenticated user selects the Completed tab
- **THEN** the system shows only tasks with status Completed, and none with another status

#### Scenario: Marking a task complete moves it out of the Active tab
- **WHEN** the authenticated user changes one of their tasks' status to Completed while viewing the Active tab
- **THEN** the task no longer appears in the Active tab and appears in the Completed tab

#### Scenario: Reopening a completed task moves it back to the Active tab
- **WHEN** the authenticated user changes a Completed task's status back to Pending or In Progress while viewing the Completed tab
- **THEN** the task no longer appears in the Completed tab and appears in the Active tab

### Requirement: Task Number Is Displayed
The system SHALL show each task's assigned number alongside its other details, in both the Active and Completed tabs.

#### Scenario: Task number shown on each task
- **WHEN** the authenticated user views a task in either tab
- **THEN** the system displays that task's assigned number alongside its title
