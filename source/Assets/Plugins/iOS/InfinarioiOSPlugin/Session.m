//
//  Session.m
//  InfinarioSDK
//
//  Created by Igi on 3/12/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import "Session.h"

@interface Session ()

@property Preferences *preferences;
@property NSTimer *timer;

@end

@implementation Session

static double const SESSION_PING = 15.0;
static long const SESSION_TIMEOUT = 120L;

- (instancetype)initWithPreferences:(Preferences *)preferences {
    self = [super init];
    
    self.preferences = preferences;
    
    return self;
}

- (void)run {
    [self ping];
    self.timer = [NSTimer scheduledTimerWithTimeInterval:SESSION_PING target:self selector:@selector(onTimer:) userInfo:nil repeats:YES];
}

- (void)ping:(BOOL)forceRestart withCustomer:(NSMutableDictionary *)customer {
    @synchronized(self) {        
        NSNumber *now = [NSNumber numberWithLong:[[NSDate date] timeIntervalSince1970]];
        NSNumber *sessionStart = @([[self.preferences objectForKey:@"session_start" withDefault:@-1] intValue]);
        NSNumber *sessionEnd = @([[self.preferences objectForKey:@"session_end" withDefault:@-1] intValue]);
        
        if ([sessionStart isEqualToNumber:@-1]) {
            [self.preferences setObject:now forKey:@"session_start"];
            [self.preferences setObject:now forKey:@"session_end"];
            
            [[NSNotificationCenter defaultCenter] postNotificationName: @"SessionStart" object:self userInfo:@{@"timestamp": now}];
        }
        else if ([sessionEnd longValue] + SESSION_TIMEOUT < [now longValue] || forceRestart) {
            [[NSNotificationCenter defaultCenter] postNotificationName: @"SessionEnd" object:self userInfo:@{
                @"timestamp": sessionEnd,
                @"duration": [NSNumber numberWithLong:([sessionEnd longValue] - [sessionStart longValue])]
            }];
            
            if (forceRestart && customer) {
                [[NSNotificationCenter defaultCenter] postNotificationName: @"SessionRestart" object:self userInfo:@{@"customer": customer}];
            }
            
            [self.preferences setObject:now forKey:@"session_start"];
            [self.preferences setObject:now forKey:@"session_end"];
            
            [[NSNotificationCenter defaultCenter] postNotificationName: @"SessionStart" object:self userInfo:@{@"timestamp": now}];
        }
        else {
            [self.preferences setObject:now forKey:@"session_end"];
        }
    }
}

- (void)ping {
    [self ping:NO withCustomer:nil];
}

- (void)restart:(NSMutableDictionary *)customer {
    [self ping:YES withCustomer:customer];
}

- (void)onTimer:(NSTimer *)timer {
    [self ping];
}

@end
