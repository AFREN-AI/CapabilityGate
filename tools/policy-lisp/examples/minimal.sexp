(policy
  (capabilities "search.read" "mail.send")
  (tool "search.read"
    (requires "search.read")
    (control user-intent host-policy)
    (argument "query" user-intent external-content))
  (tool "mail.send"
    (requires "mail.send")
    (control user-intent)
    (argument "recipient" user-intent)
    (argument "body" user-intent)))
