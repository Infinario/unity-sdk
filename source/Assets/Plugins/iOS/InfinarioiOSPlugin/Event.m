//
//  Event.m
//  InfinarioSDK
//
//  Created by Igi on 2/5/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import "Event.h"

@interface Event ()

@property NSDictionary *customerIds;
@property NSString *projectId;
@property NSDictionary *properties;
@property NSString *type;
@property NSNumber *timestamp;

@end

@implementation Event

- (instancetype)initWithIds:(NSDictionary *)ids andProjectId:(NSString *)projectId andWithType:(NSString *)type
          andWithProperties:(NSDictionary *)properties andWithTimestamp:(NSNumber *)timestamp {
    
    self = [super initWithEndpoint:@"crm/events" andTimestamp:timestamp];
    
    self.customerIds = ids;
    self.projectId = projectId;
    self.type = type;
    self.properties = properties;
    
    return self;
}

- (NSMutableDictionary *)getData {
    NSMutableDictionary *data = [[NSMutableDictionary alloc] init];
    
    data[@"customer_ids"] = self.customerIds;
    data[@"project_id"] = self.projectId;
    data[@"type"] = self.type;
    data[@"age"] = self.timestamp;
    
    if (self.properties) {
        data[@"properties"] = self.properties;
    }
    
    return data;
}

@end
