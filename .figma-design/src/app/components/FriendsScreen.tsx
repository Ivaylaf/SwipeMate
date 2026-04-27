import React, { useState } from 'react';
import { Card } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Avatar, AvatarFallback, AvatarImage } from './ui/avatar';
import { Badge } from './ui/badge';
import { ArrowLeft, UserPlus, Check, X, Search } from 'lucide-react';
import { useApp } from '../context/AppContext';
import { Tabs, TabsContent, TabsList, TabsTrigger } from './ui/tabs';

interface FriendsScreenProps {
  onBack: () => void;
}

export const FriendsScreen: React.FC<FriendsScreenProps> = ({ onBack }) => {
  const { friends, friendRequests, acceptFriendRequest, rejectFriendRequest, addFriend } = useApp();
  const [searchUsername, setSearchUsername] = useState('');

  const handleAddFriend = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchUsername.trim()) {
      addFriend(searchUsername);
      setSearchUsername('');
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-pink-50 to-purple-50">
      {/* Header */}
      <div className="bg-white border-b">
        <div className="max-w-4xl mx-auto px-4 py-4 flex items-center space-x-4">
          <Button variant="ghost" size="icon" onClick={onBack}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <h1 className="text-2xl font-bold">Friends</h1>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 py-8 space-y-6">
        {/* Add Friend */}
        <Card className="p-6">
          <h3 className="font-semibold mb-4">Add New Friend</h3>
          <form onSubmit={handleAddFriend} className="flex space-x-2">
            <Input
              placeholder="Enter username"
              value={searchUsername}
              onChange={(e) => setSearchUsername(e.target.value)}
              className="flex-1"
            />
            <Button type="submit" className="bg-gradient-to-r from-pink-500 to-purple-500">
              <UserPlus className="h-5 w-5 mr-2" />
              Add
            </Button>
          </form>
        </Card>

        {/* Tabs */}
        <Tabs defaultValue="friends" className="w-full">
          <TabsList className="grid w-full grid-cols-2">
            <TabsTrigger value="friends">
              My Friends ({friends.length})
            </TabsTrigger>
            <TabsTrigger value="requests" className="relative">
              Requests
              {friendRequests.length > 0 && (
                <span className="ml-2 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center">
                  {friendRequests.length}
                </span>
              )}
            </TabsTrigger>
          </TabsList>

          <TabsContent value="friends" className="space-y-4">
            {friends.length === 0 ? (
              <Card className="p-12 text-center">
                <p className="text-gray-500">No friends yet. Start adding some!</p>
              </Card>
            ) : (
              <div className="space-y-3">
                {friends.map((friend) => (
                  <Card key={friend.id} className="p-4">
                    <div className="flex items-center space-x-4">
                      <Avatar className="h-12 w-12">
                        <AvatarImage src={friend.profilePicture} />
                        <AvatarFallback>
                          {friend.username.substring(0, 2).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1">
                        <p className="font-semibold">{friend.username}</p>
                        <Badge
                          variant={friend.status === 'active' ? 'default' : 'secondary'}
                          className={friend.status === 'active' ? 'bg-green-500' : ''}
                        >
                          {friend.status === 'active' ? 'Online' : 'Offline'}
                        </Badge>
                      </div>
                    </div>
                  </Card>
                ))}
              </div>
            )}
          </TabsContent>

          <TabsContent value="requests" className="space-y-4">
            {friendRequests.length === 0 ? (
              <Card className="p-12 text-center">
                <p className="text-gray-500">No pending friend requests</p>
              </Card>
            ) : (
              <div className="space-y-3">
                {friendRequests.map((request) => (
                  <Card key={request.id} className="p-4">
                    <div className="flex items-center space-x-4">
                      <Avatar className="h-12 w-12">
                        <AvatarImage src={request.fromUser.profilePicture} />
                        <AvatarFallback>
                          {request.fromUser.username.substring(0, 2).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1">
                        <p className="font-semibold">{request.fromUser.username}</p>
                        <p className="text-sm text-gray-500">
                          {new Date(request.timestamp).toLocaleDateString()}
                        </p>
                      </div>
                      <div className="flex space-x-2">
                        <Button
                          size="sm"
                          onClick={() => acceptFriendRequest(request.id)}
                          className="bg-green-500 hover:bg-green-600"
                        >
                          <Check className="h-4 w-4" />
                        </Button>
                        <Button
                          size="sm"
                          variant="destructive"
                          onClick={() => rejectFriendRequest(request.id)}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  </Card>
                ))}
              </div>
            )}
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
};
