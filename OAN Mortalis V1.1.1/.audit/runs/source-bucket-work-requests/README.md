# SOURCE_BUCKET_WORK_REQUESTS

This directory is the line-local Build outbox for bounded source-bucket work.

Each request should be emitted into its own bundle directory and should carry:

- `request.json`
- `request.md`

Build publishes need here.

Source buckets read, classify, and return receipts from their own lawful local
surfaces.

Build does not mutate external source buckets from this outbox.
