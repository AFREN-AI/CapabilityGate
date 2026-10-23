(policy
  (capabilities "search.read")
  (tool "search.read"
    (requires "search.read")
    (control user-intent)
    (argument "query" (load "untrusted-file"))))
