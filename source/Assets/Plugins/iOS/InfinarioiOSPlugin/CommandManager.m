//
//  CommandManager.m
//  InfinarioSDK
//
//  Created by Igi on 2/5/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import "CommandManager.h"
#import "DbQueue.h"
#import "Http.h"
#import "Preferences.h"
#import "Device.h"


int const MAX_RETRIES = 50;

@interface CommandManager ()

@property DbQueue *dbQueue;
@property Http *http;
@property NSString *token;
@property Preferences *preferences;

@end

@implementation CommandManager

- (instancetype)initWithTarget:(NSString *)target andWithToken:(NSString *)token {
    self = [super init];
    
    self.dbQueue = [[DbQueue alloc] init];
    self.http = [[Http alloc] initWithTarget: target];
    self.token = token;
    self.preferences = [Preferences sharedInstance];
    
    return self;
}

- (void)schedule:(Command *)command {
    [self.dbQueue schedule:[command getPayload]];
}

- (void)flush {
    int retries = MAX_RETRIES;
    
    while (retries > 0) {
        if (![self executeBatch]) {
            if ([self.dbQueue isEmpty]) {
                break;
            }
            else {
                retries--;
            }
        }
    }
}

- (NSNumber *)nowInSeconds {
    return [NSNumber numberWithLong:[[NSDate date] timeIntervalSince1970]];
}

- (void)setAge:(NSMutableDictionary *)command {
    if (command[@"data"] && command[@"data"][@"age"]) {
        command[@"data"][@"age"] = [NSNumber numberWithLong:[[self nowInSeconds] longValue] - [command[@"data"][@"age"] longValue]];
    }
}

- (void)setCookieId:(NSMutableDictionary *)command {
    if (command[@"data"] && command[@"data"][@"ids"] && ![command[@"data"][@"ids"][@"cookie"] length]) {
        command[@"data"][@"ids"][@"cookie"] = [self.preferences objectForKey:@"campaignCookie"];
    }
    
    if (command[@"data"] && command[@"data"][@"customer_ids"] && ![command[@"data"][@"customer_ids"][@"cookie"] length]) {
        command[@"data"][@"customer_ids"][@"cookie"] = [self.preferences objectForKey:@"campaignCookie"];
    }
}

- (BOOL)ensureCookieId {
    NSString *campaignCookie = [self.preferences objectForKey:@"campaignCookie" withDefault:@""];
    
    if ([campaignCookie isEqualToString:@""]) {
        CFUUIDRef uuid = CFUUIDCreate(kCFAllocatorDefault);
        campaignCookie = (__bridge_transfer NSString *)CFUUIDCreateString(kCFAllocatorDefault, uuid);
        CFRelease(uuid);
        
        NSDictionary *response = [self.http post:@"crm/customers/track" withPayload:@{
            @"ids": @{@"cookie": campaignCookie},
            @"project_id": self.token,
            @"device": [Device deviceProperties]
        }];
        
        if (response) {
            campaignCookie = response[@"data"][@"ids"][@"cookie"];
            NSLog(@"Negotiated cookie id");
            [self.preferences setObject:campaignCookie forKey:@"campaignCookie"];
            
            NSString *cookie = [self.preferences objectForKey:@"cookie" withDefault:@""];
            
            if ([cookie isEqualToString:@""]) {
                [self.preferences setObject:campaignCookie forKey:@"cookie"];
            }
            
            return YES;
        }
        
        return NO;
    }
    
    NSString *cookie = [self.preferences objectForKey:@"cookie" withDefault:@""];
    
    if ([cookie isEqualToString:@""]) {
        [self.preferences setObject:campaignCookie forKey:@"cookie"];
    }
    
    return YES;
}

- (BOOL)executeBatch {
    if (![self ensureCookieId]) {
        NSLog(@"Failed to negotiate cookie id.");
        return NO;
    }
    
    NSMutableSet *successful = [[NSMutableSet alloc] init];
    NSMutableSet *failed = [[NSMutableSet alloc] init];
    NSArray *requests = [self.dbQueue pop];
    NSMutableArray *commands = [[NSMutableArray alloc] init];
    NSMutableDictionary *request;
    NSDictionary *result;
    NSString *status;
    
    if (![requests count]) return NO;
    
    for (NSDictionary *req in requests) {
        [self setAge:req[@"command"]];
        [self setCookieId:req[@"command"]];
        [commands addObject:req[@"command"]];
        [failed addObject:req[@"id"]];
    }
    
    NSDictionary *response = [self.http post:@"bulk" withPayload:@{@"commands": commands}];
    
    if (response && response[@"results"]) {
        for (int i = 0; i < [response[@"results"] count] && i < [requests count]; ++i) {
            request = requests[i];
            result = response[@"results"][i];
            status = [result[@"status"] lowercaseString];
            
            if ([status isEqualToString:@"ok"]) {
                [failed removeObject:request[@"id"]];
                [successful addObject:request[@"id"]];
            }
            else if ([status isEqualToString:@"retry"]) {
                [failed removeObject:request[@"id"]];
            }
        }
    }
    
    [self.dbQueue clear:[successful allObjects] andFailed:[failed allObjects]];
    
    NSLog(@"Sent commands: %d successful, %d failed out of %d", (int) [successful count], (int) [failed count], (int) [requests count]);
    
    return [successful count] > 0 || [failed count] > 0;
}

@end
