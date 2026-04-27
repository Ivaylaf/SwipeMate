# SwipeMate - Project Summary

## Overview
SwipeMate is a modern, mobile-first web application that helps groups of friends make collaborative decisions using an intuitive swipe-based interface. The app covers four main categories: Movies/TV, Restaurants, Recipes, and Board Games.

## Technology Stack
- **Framework**: React 18.3 with TypeScript
- **Styling**: Tailwind CSS 4.0
- **UI Components**: Radix UI primitives
- **Animations**: Motion (Framer Motion)
- **Icons**: Lucide React
- **Build Tool**: Vite

## Key Features Implemented

### 1. Authentication System
- Login screen with gradient branding
- Registration screen with form validation
- Mock authentication (frontend demo)
- Persistent user session

### 2. Home Dashboard
- Four category cards with images and gradients:
  - 🎬 Movies & TV Series
  - 🍽️ Restaurants (Bulgaria focus)
  - 👨‍🍳 Recipes
  - 🎲 Board Games
- Quick access buttons for Friends, History, and Profile
- Friend request notifications badge

### 3. Friends Management
- Add friends by username
- View friends list with online/offline status
- Friend requests system with accept/reject
- Profile avatars with fallback initials

### 4. Match Session Creation
- Category selection
- Friend selection with checkboxes
- Visual feedback for selected friends
- Ability to start solo or group sessions

### 5. Dynamic Filters
Category-specific filter systems:
- **Movies**: Genre, rating, year, duration
- **Restaurants**: Cuisine, district (Sofia), rating
- **Recipes**: Difficulty, cuisine, meal type, budget
- **Board Games**: Game type, player count, duration, rating

### 6. Swipe Interface
- Touch-friendly card-based swiping
- Drag gestures with Motion animations
- Like (❤️) and Reject (✕) buttons
- Progress indicator (X/Y items)
- Smooth transitions between cards
- Rotation and opacity effects on drag

### 7. Match Results
- Celebration screen with animated icon
- Match details with ratings and metadata
- Participant avatars
- Call-to-action to return home
- Entrance animation

### 8. Match History
- List of past group decisions
- Color-coded by category
- Participant avatars
- Timestamp display (relative dates)
- Quick view of match details

### 9. User Profile
- User stats dashboard:
  - Total matches
  - Reviews written
  - Ratings given
- Badge system:
  - Reviewer Level 1 (10+ reviews)
  - Active User (50+ ratings)
- Progress bars toward next achievements
- Profile picture with fallback

### 10. UI/UX Features
- Mobile-first responsive design
- Gradient color schemes per category
- Rounded cards and modern styling
- Loading states and empty states
- Toast notifications (Sonner)
- Welcome tour for new users
- Smooth page transitions

## Application Flow

```
Login/Register
    ↓
Home Dashboard
    ↓
Select Category → Select Friends → Set Filters → Swipe Cards → Match Result
    ↓                                                ↓
History ←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←
```

## Mock Data Structure

### Movies (5 items)
- Classic and modern films
- TV series examples
- Ratings, genres, years, duration
- Descriptions

### Restaurants (5 items)
- Real Sofia locations
- Bulgarian and international cuisine
- Districts, ratings, price ranges
- Descriptions

### Recipes (5 items)
- Various difficulty levels
- Multiple cuisines
- Meal types (main, appetizer, dessert)
- Prep times and ingredient lists

### Board Games (5 items)
- Popular titles
- Game types (strategy, party, etc.)
- Player counts
- Complexity ratings

### Users & Friends
- Current user with profile
- 4 mock friends with avatars
- 2 pending friend requests
- Pre-populated match history

## Component Architecture

```
App.tsx (Router & State Management)
├── AppContext (Global State)
├── LoginScreen
├── RegisterScreen
├── HomeScreen
├── FriendsScreen
├── CreateSessionScreen
├── FiltersScreen
├── SwipeScreen
├── MatchResultScreen
├── HistoryScreen
├── ProfileScreen
└── WelcomeTour
```

## State Management
- React Context API for global state
- Local component state for UI interactions
- Session tracking for swipe progress
- Match detection logic
- History persistence (in-memory)

## Design System
- **Primary Colors**: Pink to Purple gradients
- **Category Colors**:
  - Movies: Blue to Cyan
  - Restaurants: Orange to Red
  - Recipes: Green to Emerald
  - Board Games: Purple to Pink
- **Typography**: System defaults with custom weights
- **Spacing**: Consistent padding and margins
- **Radius**: Rounded corners (10px default)

## Animations
- Card swipe with drag physics
- Rotation on drag
- Opacity fade on exit
- Match result entrance
- Icon wiggle on match
- Smooth page transitions

## Future Backend Integration Points
When connecting to Supabase or similar backend:
1. **Authentication**: Replace mock with real auth
2. **Real-time**: Add WebSocket for live session sync
3. **Database Tables**:
   - users
   - friends
   - friend_requests
   - match_sessions
   - swipe_actions
   - match_results
   - reviews
4. **Push Notifications**: Session invites and match alerts
5. **Content API**: Dynamic content from database
6. **User Preferences**: Saved filters and favorites

## File Structure
```
/src
├── /app
│   ├── App.tsx
│   ├── /components
│   │   ├── LoginScreen.tsx
│   │   ├── RegisterScreen.tsx
│   │   ├── HomeScreen.tsx
│   │   ├── FriendsScreen.tsx
│   │   ├── CreateSessionScreen.tsx
│   │   ├── FiltersScreen.tsx
│   │   ├── SwipeScreen.tsx
│   │   ├── MatchResultScreen.tsx
│   │   ├── HistoryScreen.tsx
│   │   ├── ProfileScreen.tsx
│   │   ├── WelcomeTour.tsx
│   │   └── /ui (Radix components)
│   ├── /context
│   │   └── AppContext.tsx
│   ├── /types
│   │   └── index.ts
│   └── /data
│       └── mockData.ts
└── /styles
    ├── tailwind.css
    ├── theme.css
    └── fonts.css
```

## Demo Usage
1. Login with any username/password
2. Welcome tour appears
3. Choose a category from home
4. Select friends (optional)
5. Set filters (optional)
6. Swipe through items
7. Like an item to create a match
8. View match result
9. Check history and profile

## Performance Considerations
- Lazy loading for images
- Optimized re-renders with React.memo (where needed)
- Efficient state updates
- CSS-based animations
- Code splitting potential for routes

## Accessibility
- Semantic HTML structure
- ARIA labels on interactive elements
- Keyboard navigation support
- Focus management
- Screen reader friendly
- Color contrast compliance

## Browser Compatibility
- Modern browsers (Chrome, Firefox, Safari, Edge)
- Mobile browsers (iOS Safari, Chrome Mobile)
- Responsive breakpoints for tablets and desktops
- Touch and mouse event support

## Known Limitations (Demo Version)
- No real-time synchronization
- Mock authentication only
- In-memory data (resets on refresh)
- Limited content library (5 items per category)
- No actual API integrations
- Simulated match detection
- No persistent storage

## Next Steps for Production
1. Connect Supabase for backend
2. Implement real-time session sync
3. Add content APIs or databases
4. Implement push notifications
5. Add user settings and preferences
6. Expand content libraries
7. Add rating and review functionality
8. Implement chat in sessions
9. Add social features
10. Performance monitoring and analytics

---

**Built with ❤️ using React, TypeScript, and Tailwind CSS**
