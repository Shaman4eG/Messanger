# What'sVibeGram 
Messenger application, which provides possibilities for text and files exchange.

## Features
- Signing up and signing in.
- Self-chat, which can be used to store your files/texts. 
- Personal chat between 2 users.
- Group chat with up to 10 users simultaneously.

## Soon to be implemented fully or added. 
- Profile with user information.
- Files exchange.
- Self-deletable messages.
- History search.
- Message resending.

## TODO (exact place of need for these points can be found near appropriate classes)
#### Server
- Provide exact reasoning on input failure.
- Extend tests coverage.
- Add better assert implementations to several test methods.
#### WPF Client
- Bind code-nehind XAML pages with server API.
- Make all API calls async.
- Validate input in client also.

## Known issues
- Sometime messages in chat are not in by date order.
- Attachments can be fetched to other chats beside the one where they were initial sent.
- Messages doubling and loss happens, when simultaneously messages from different clients are sent.
- Too big images are not fully visualized.
