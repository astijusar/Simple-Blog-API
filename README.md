# Simple Blog REST API

Simple blog implementation that uses ASP.NET Core to create an API that follows REST rules.

Features:
- Global Error Handling
- Validation
- Asynchronus Code
- Action Filters
- Swagger documentation
- Dto use

The API has 3 controllers:
- Categories
- Posts
- Comments

### Categories:

| **Method**  | **Route**  |
| ------------ | ------------ |
| **GET**  | *api/categories*  |
|  **GET** | *api/categories/{categoryId}*  |
| **GET**  |  *api/categories/({ids})* |
| **POST**  | *api/categories*  |
|  **POST** | *api/categories/collection*  |
| **PUT**  | *api/categories/{categoryId}*  |
| **PATCH**  | *api/categories/{categoryId}*  |
| **DELETE**  |  *api/categories/{categoryId}* |

### Posts:

| **Method**  | **Route**  |
| ------------ | ------------ |
| **GET**  | *api/posts*  |
|  **GET** | *api/categories/{categoryId}/posts*  |
| **GET**  |  *api/posts/{postId}* |
|  **GET** | *api/posts/collection/{ids}*  |
| **POST**  | *api/categories/{categoryId}/posts*  |
|  **POST** | *api/categories/{categoryId}/posts/collection*  |
| **PUT**  | *api/post/{postId}*  |
| **PATCH**  | *api/post/{postId}*  |
| **DELETE**  |  *api/posts/{postId}* |

### Comments:

| **Method**  | **Route**  |
| ------------ | ------------ |
| **GET**  | *api/posts/{postId}/comments*  |
|  **GET** | *api/posts/{postId}/comments/{commentId}*  |
| **GET**  |  *api/posts/{postId}/comments/collection/({ids})* |
| **POST**  | *api/posts/{postId}/comments*  |
| **PUT**  | *api/posts/{postId}/comments/{commentId}*  |
| **PATCH**  | *api/posts/{postId}/comments/{commentId}*  |
| **DELETE**  |  *api/posts/{postId}/comments/{commentId}* |

-----
# More detailed description of all routes down below.
-----

# /api/Categories

### GET
#### Summary:

Get a list of all categories

#### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns the categories list |

-----
### POST
#### Summary:

Creates a new category

##### Responses

| Code | Description |
| ---- | ----------- |
| 201 | Returns a newly created category |
| 400 | Category input object sent from client is null |
| 422 | Invalid model state for the category input object |

# /api/Categories/{categoryId}

### GET
#### Summary:

Get the category by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| categoryId | path | Category id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns the category found by id |
| 404 | The category does not exist |

-----
### DELETE
#### Summary:

Deletes a category by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| categoryId | path | Category id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 204 | Returns 204 no content response |
| 404 | The category does not exist |

-----
### PUT
#### Summary:

Update the whole category by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| categoryId | path | Category id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns 204 no content response |
| 400 | Category input is null |
| 404 | The category does not exist |
| 422 | Invalid model state for the category input |

-----
### PATCH
#### Summary:

Partially update category by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| categoryId | path | Category id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 204 | Returns 204 no content response |
| 400 | Category input object sent from client is null |
| 404 | Category does not exist |
| 422 | Invalid model state for the category input object |

# /api/Categories/collection/({ids})

### GET
#### Summary:

Get a list of categories by ids

##### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| ids | query | Ids of categories | Yes | [ integer ] |

##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns the categories found by ids |
| 400 | Parameter ids is null |
| 404 | Some ids are not valid in a collection |

# /api/Categories/collection

### POST
#### Summary:

Creates multiple new categories

#### Responses

| Code | Description |
| ---- | ----------- |
| 201 | Returns newly created categories |
| 400 | Categories collection input object sent from client is null |
| 422 | Invalid model state for the category input object |

# /api/posts/{postId}/Comments

### GET
#### Summary:

Get a list of comments from a specific post

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| postId | path | Post id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns the comments |
| 404 | The post does not exist |

----
### POST
#### Summary:

Create comment for post

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| postId | path | Post id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 201 | Returns the newly created comment |
| 400 | Comment input object is null |
| 404 | Post does not exist |
| 422 | Invalid model state for comment input object |

# /api/posts/{postId}/Comments/{commentId}

### GET
#### Summary:

Get a specific comment from a specific post

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| postId | path | Post id | Yes | integer |
| commentId | path | Comment id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns the comment |
| 404 | The post or comment does not exist |

-----
### DELETE
#### Summary:

Delete comment by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| postId | path | post id | Yes | integer |
| commentId | path | comment id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 204 | Returns no content response |
| 404 | Post or comment does not exist |

-----
### PUT
#### Summary:

Update comment for post by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| postId | path | Post id | Yes | integer |
| commentId | path | Comment id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 204 | Returns no content response |
| 400 | Comment input object is null |
| 404 | Post or comment does not exist |
| 422 | Invalid model state for comment input object |

-----
### PATCH
#### Summary:

Partially update comment for post by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| postId | path | Post id | Yes | integer |
| commentId | path | Comment id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 204 | Returns no content response |
| 400 | Comment input object sent from client is null |
| 404 | Post or comment does not exist |
| 422 | Invalid model state for the comment input object |

# /api/posts/{postId}/Comments/collection/({ids})

### GET
#### Summary:

Get a list of comments by ids

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| ids | query | Ids of comments | Yes | [ integer ] |
| postId | path |  | Yes | string |

#### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns a list of comments |
| 400 | Parameter ids is null |
| 404 | Some ids are not valid in a collection |

# /api/categories/{categoryId}/Posts

### GET
#### Summary:

Get a list of posts for category

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| categoryId | path | Category id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns a list of posts |
| 404 | Category does not exist |

-----
### POST
#### Summary:

Create a post for category

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| categoryId | path | Category id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 201 | Returns the newly created post |
| 400 | Post input object is null |
| 404 | Category does not exist |
| 422 | Invalid model state for post input object |

# /api/Posts

### GET
#### Summary:

Get a list of all posts regardless of category

#### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns a list of posts |

# /api/Posts/{postId}

### GET
#### Summary:

Get a post by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| postId | path | post id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns a posts |
| 404 | Post does not exist |

-----
### DELETE
#### Summary:

Delete a post by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| postId | path | Post id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 204 | Returns no content response |
| 404 | Post does not exist |

-----
### PUT
#### Summary:

Update a post by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| postId | path | Post id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 204 | Returns no content response |
| 400 | Post input object is null |
| 404 | Post does not exist |
| 422 | Invalid model state for post input object |

-----
### PATCH
#### Summary:

Partially update post by id

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| postId | path | Post id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 204 | Returns no content response |
| 400 | Post input object sent from client is null |
| 404 | Post does not exist |
| 422 | Invalid model state for the post input object |

# /api/Posts/collection/({ids})

### GET
#### Summary:

Get a list of posts by ids

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| ids | query | Ids of posts | Yes | [ integer ] |

#### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Returns a list of posts |
| 400 | Parameter ids is null |
| 404 | Some ids are not valid in a collection |

# /api/categories/{categoryId}/Posts/collection

### POST
#### Summary:

Create multiple posts for category

#### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| categoryId | path | Category id | Yes | integer |

#### Responses

| Code | Description |
| ---- | ----------- |
| 201 | Returns the newly created posts |
| 400 | Post input objects are null |
| 404 | Category does not exist |
| 422 | Invalid model state for post input object(s) |
